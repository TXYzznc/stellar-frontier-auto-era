using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoEra.Save
{
    public enum WorldSaveState { Idle, WaitingBoundary, Saving, Saved, Failed }
    public readonly struct WorldSaveWriteResult
    {
        public WorldSaveWriteResult(bool succeeded, string reason = null) { Succeeded = succeeded; Reason = reason; }
        public bool Succeeded { get; }
        public string Reason { get; }
    }
    public interface IWorldSnapshotSource
    {
        // Main thread only. False means no complete safe boundary is currently available.
        bool TryCapture(long revision, out WorldSnapshotDocument snapshot, out string reason);
    }
    public interface IWorldSnapshotWriter
    {
        Task<WorldSaveWriteResult> WriteAsync(int slotIndex, WorldSnapshotDocument snapshot, CancellationToken cancellationToken);
    }
    public sealed class SaveSlotWorldSnapshotWriter : IWorldSnapshotWriter
    {
        private readonly SaveSlotService _slots;
        private readonly SemaphoreSlim[] _slotsInFlight = { new SemaphoreSlim(1,1), new SemaphoreSlim(1,1), new SemaphoreSlim(1,1) };
        public SaveSlotWorldSnapshotWriter(SaveSlotService slots) { _slots = slots ?? throw new ArgumentNullException(nameof(slots)); }
        public Task<WorldSaveWriteResult> WriteAsync(int slotIndex, WorldSnapshotDocument snapshot, CancellationToken cancellationToken)
        { return WriteCoreAsync(slotIndex, snapshot, null, false, cancellationToken); }

        public Task<WorldSaveWriteResult> WriteOfflineAsync(int slotIndex, OfflineWorldCheckpoint checkpoint, CancellationToken cancellationToken)
        {
            if (checkpoint == null) throw new ArgumentNullException(nameof(checkpoint));
            return WriteCoreAsync(slotIndex, checkpoint.Document, checkpoint.SavedUtcWatermark, checkpoint.Pending, cancellationToken);
        }

        private async Task<WorldSaveWriteResult> WriteCoreAsync(int slotIndex, WorldSnapshotDocument snapshot,
            DateTimeOffset? savedUtc, bool offlinePending, CancellationToken cancellationToken)
        {
            if (!SaveSlotService.IsValidSlotIndex(slotIndex)) throw new ArgumentOutOfRangeException(nameof(slotIndex));
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            await _slotsInFlight[slotIndex].WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string content = WorldSnapshotCodec.Serialize(snapshot);
                    cancellationToken.ThrowIfCancellationRequested();
                    // Once atomic file IO starts, finish it. Cancellation must not interrupt replacement.
                    bool succeeded = savedUtc.HasValue
                        ? _slots.OverwriteAtUtc(slotIndex, snapshot.Summary, snapshot.WorldMilliseconds, content, offlinePending, savedUtc.Value)
                        : _slots.Overwrite(slotIndex,snapshot.Summary,snapshot.WorldMilliseconds,content);
                    return new WorldSaveWriteResult(succeeded,succeeded ? null : "存档写入或校验失败");
                },cancellationToken).ConfigureAwait(false);
            }
            finally { _slotsInFlight[slotIndex].Release(); }
        }
    }

    /// <summary>One world/slot owner. Main-thread capture and polling; background work receives detached DTOs only.</summary>
    public sealed class WorldSaveCoordinator : IDisposable
    {
        public const double AutosaveIntervalSeconds = 60;
        private readonly IWorldSnapshotSource _source;
        private readonly IWorldSnapshotWriter _writer;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private Task<WorldSaveWriteResult> _write;
        private long _writeRevision;
        private bool _requested, _force, _disposed;
        private double _period;
        public WorldSaveCoordinator(int slotIndex, IWorldSnapshotSource source, IWorldSnapshotWriter writer, long savedRevision = 0)
        {
            if (!SaveSlotService.IsValidSlotIndex(slotIndex) || savedRevision < 0) throw new ArgumentOutOfRangeException(nameof(slotIndex));
            SlotIndex = slotIndex; _source = source ?? throw new ArgumentNullException(nameof(source)); _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            DirtyRevision = SavedRevision = savedRevision;
        }
        public int SlotIndex { get; }
        public long DirtyRevision { get; private set; }
        public long SavedRevision { get; private set; }
        public bool HasUnsavedChanges => DirtyRevision > SavedRevision;
        public bool IsWriting => _write != null;
        public bool HasPendingRequest => _requested;
        public WorldSaveState State { get; private set; }
        public string Reason { get; private set; }
        public event Action Changed;
        public event Action SnapshotCaptured;
        public void MarkDirty(bool requestSave = false)
        {
            if (_disposed) return;
            DirtyRevision = checked(DirtyRevision + 1);
            if (requestSave) RequestSave();
        }
        public void RequestSave(bool force = false)
        {
            if (_disposed) return;
            _requested = true; _force |= force;
        }
        public void Pump(double realElapsedSeconds)
        {
            if (_disposed) return;
            if (double.IsNaN(realElapsedSeconds) || double.IsInfinity(realElapsedSeconds) || realElapsedSeconds < 0) throw new ArgumentOutOfRangeException(nameof(realElapsedSeconds));
            _period += realElapsedSeconds;
            if (_period >= AutosaveIntervalSeconds)
            { _period %= AutosaveIntervalSeconds; if (HasUnsavedChanges) _requested = true; }
            if (_write != null && _write.IsCompleted)
            {
                var completed = _write; _write = null;
                try
                {
                    var result = completed.GetAwaiter().GetResult();
                    if (result.Succeeded) { SavedRevision = Math.Max(SavedRevision,_writeRevision); SetState(WorldSaveState.Saved,null); }
                    else SetState(WorldSaveState.Failed,result.Reason ?? "保存失败");
                }
                catch (Exception exception) { SetState(WorldSaveState.Failed,"保存失败：" + exception.GetType().Name); }
            }
            if (_write != null || !_requested) return;
            if (!HasUnsavedChanges && !_force) { _requested = false; return; }
            long revision = DirtyRevision;
            WorldSnapshotDocument snapshot; string reason;
            bool captured;
            try { captured = _source.TryCapture(revision,out snapshot,out reason); }
            catch (Exception exception)
            { SetState(WorldSaveState.Failed,"快照捕获失败：" + exception.GetType().Name); _requested = false; _force = false; return; }
            if (!captured)
            { SetState(WorldSaveState.WaitingBoundary,reason ?? "等待完整业务提交边界"); return; }
            if (snapshot == null || snapshot.Revision != revision)
            { SetState(WorldSaveState.Failed,"快照修订与保存请求不一致"); _requested = false; _force = false; return; }
            _writeRevision = revision; _requested = false; _force = false;
            try
            {
                _write = _writer.WriteAsync(SlotIndex,snapshot,_cancellation.Token);
                if (_write == null) throw new InvalidOperationException("Writer returned no task.");
                SetState(WorldSaveState.Saving,null);
                SnapshotCaptured?.Invoke();
            }
            catch (Exception exception) { SetState(WorldSaveState.Failed,"保存启动失败：" + exception.GetType().Name); }
        }
        private void SetState(WorldSaveState state, string reason)
        {
            if (State == state && Reason == reason) return;
            State = state; Reason = reason; Changed?.Invoke();
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true; _cancellation.Cancel(); _cancellation.Dispose();
            Changed = null; SnapshotCaptured = null; _requested = false;
        }
    }
}
