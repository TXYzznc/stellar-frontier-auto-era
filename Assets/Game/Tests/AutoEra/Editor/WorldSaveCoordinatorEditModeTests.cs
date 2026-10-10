using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoEra.Save;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class WorldSaveCoordinatorEditModeTests
    {
        private sealed class Source : IWorldSnapshotSource
        {
            internal bool Safe=true, Throw, Mismatch;
            internal int Captures, Value;
            public bool TryCapture(long revision,out WorldSnapshotDocument snapshot,out string reason)
            {
                if (Throw) throw new InvalidOperationException("Fixture capture fault");
                snapshot=null;reason="Fixture unsafe boundary"; if(!Safe) return false;
                Captures++; snapshot=new WorldSnapshotDocument(1000,42,Mismatch?revision+1:revision,"Fixture",new[] {new WorldSnapshotSection("machines",1,new {value=Value})}); reason=null;return true;
            }
        }
        private sealed class DelayedWriter : IWorldSnapshotWriter
        {
            internal readonly List<WorldSnapshotDocument> Snapshots=new List<WorldSnapshotDocument>();
            internal readonly List<TaskCompletionSource<WorldSaveWriteResult>> Writes=new List<TaskCompletionSource<WorldSaveWriteResult>>();
            internal CancellationToken Token;
            public Task<WorldSaveWriteResult> WriteAsync(int slot,WorldSnapshotDocument snapshot,CancellationToken token)
            { Token=token; Snapshots.Add(snapshot);var pending=new TaskCompletionSource<WorldSaveWriteResult>();Writes.Add(pending);return pending.Task; }
            internal void Complete(int index,bool success=true)=>Writes[index].SetResult(new WorldSaveWriteResult(success,success?null:"Fixture disk failure"));
        }
        [Test] public void PeriodicSave_WaitsSixtySecondsAndSkipsCleanWorld()
        {
            var source=new Source();var writer=new DelayedWriter();using(var coordinator=new WorldSaveCoordinator(0,source,writer))
            { coordinator.Pump(60);Assert.That(writer.Writes,Is.Empty);coordinator.MarkDirty();coordinator.Pump(59);Assert.That(writer.Writes,Is.Empty);coordinator.Pump(1);Assert.That(writer.Writes.Count,Is.EqualTo(1)); }
        }
        [Test] public void OldWriteCannotClearNewDirtyRevision_RequestsCoalesce()
        {
            var source=new Source {Value=1};var writer=new DelayedWriter();using(var coordinator=new WorldSaveCoordinator(0,source,writer))
            {
                coordinator.MarkDirty(true);coordinator.Pump(0);Assert.That(writer.Writes.Count,Is.EqualTo(1));
                source.Value=2;coordinator.MarkDirty(true);coordinator.RequestSave();coordinator.Pump(0);Assert.That(writer.Writes.Count,Is.EqualTo(1));
                writer.Complete(0);coordinator.Pump(0);Assert.That(coordinator.SavedRevision,Is.EqualTo(1));Assert.That(coordinator.HasUnsavedChanges,Is.True);Assert.That(writer.Writes.Count,Is.EqualTo(2));
                Assert.That(WorldSnapshotCodec.Serialize(writer.Snapshots[0]),Does.Contain("\"value\":1"));
                Assert.That(WorldSnapshotCodec.Serialize(writer.Snapshots[1]),Does.Contain("\"value\":2"));
                writer.Complete(1);coordinator.Pump(0);Assert.That(coordinator.HasUnsavedChanges,Is.False);Assert.That(coordinator.SavedRevision,Is.EqualTo(2));
            }
        }
        [Test] public void UnsafeBoundary_DefersWithoutStartingWriter()
        {
            var source=new Source {Safe=false};var writer=new DelayedWriter();using(var coordinator=new WorldSaveCoordinator(0,source,writer))
            { coordinator.MarkDirty(true);coordinator.Pump(0);Assert.That(coordinator.State,Is.EqualTo(WorldSaveState.WaitingBoundary));Assert.That(writer.Writes,Is.Empty);source.Safe=true;coordinator.Pump(0);Assert.That(writer.Writes.Count,Is.EqualTo(1)); }
        }
        [Test] public void WriteFailure_KeepsDirtyAndSupportsExplicitRetry()
        {
            var source=new Source();var writer=new DelayedWriter();using(var coordinator=new WorldSaveCoordinator(0,source,writer))
            { coordinator.MarkDirty(true);coordinator.Pump(0);writer.Complete(0,false);coordinator.Pump(0);Assert.That(coordinator.State,Is.EqualTo(WorldSaveState.Failed));Assert.That(coordinator.HasUnsavedChanges,Is.True);Assert.That(coordinator.Reason,Does.Contain("disk"));coordinator.RequestSave();coordinator.Pump(0);writer.Complete(1);coordinator.Pump(0);Assert.That(coordinator.State,Is.EqualTo(WorldSaveState.Saved)); }
        }
        [Test] public void CleanExitCanForceSnapshotAndFreezeOnlyAfterCapture()
        {
            var source=new Source {Safe=false};var writer=new DelayedWriter();using(var coordinator=new WorldSaveCoordinator(0,source,writer))
            { int captures=0;coordinator.SnapshotCaptured+=()=>captures++;coordinator.RequestSave(true);coordinator.Pump(0);Assert.That(captures,Is.Zero);source.Safe=true;coordinator.Pump(0);Assert.That(captures,Is.EqualTo(1));Assert.That(coordinator.IsWriting,Is.True); }
        }
        [Test] public void CaptureFault_DoesNotWriteOrMarkWorldSaved()
        {
            var source=new Source {Throw=true};var writer=new DelayedWriter();using(var coordinator=new WorldSaveCoordinator(0,source,writer))
            { coordinator.MarkDirty(true);coordinator.Pump(0);Assert.That(coordinator.State,Is.EqualTo(WorldSaveState.Failed));Assert.That(coordinator.HasUnsavedChanges,Is.True);Assert.That(writer.Writes,Is.Empty); }
        }
        [Test] public void MismatchedSnapshotRevision_IsRejected()
        {
            var source=new Source {Mismatch=true};var writer=new DelayedWriter();using(var coordinator=new WorldSaveCoordinator(0,source,writer))
            { coordinator.MarkDirty(true);coordinator.Pump(0);Assert.That(coordinator.State,Is.EqualTo(WorldSaveState.Failed));Assert.That(writer.Writes,Is.Empty); }
        }
        [Test] public void DisposedOwner_CancelsQueuedWorkAndNoLongerPublishes()
        {
            var source=new Source();var writer=new DelayedWriter();var coordinator=new WorldSaveCoordinator(0,source,writer);
            coordinator.MarkDirty(true);coordinator.Pump(0);int notifications=0;coordinator.Changed+=()=>notifications++;
            coordinator.Dispose();Assert.That(writer.Token.IsCancellationRequested,Is.True);writer.Complete(0);coordinator.Pump(60);Assert.That(notifications,Is.Zero);Assert.That(source.Captures,Is.EqualTo(1));
        }
        [Test] public async Task BackgroundWriter_UsesExistingChecksumsAndRollingBackups()
        {
            string directory=Path.Combine(Path.GetTempPath(),"AutoEra-B47-"+Guid.NewGuid().ToString("N"));
            try
            {
                var slots=new SaveSlotService(directory);var writer=new SaveSlotWorldSnapshotWriter(slots);var source=new Source {Value=1};
                Assert.That(source.TryCapture(1,out var first,out _),Is.True);
                Assert.That((await writer.WriteAsync(0,first,CancellationToken.None)).Succeeded,Is.True);
                source.Value=2;Assert.That(source.TryCapture(2,out var second,out _),Is.True);
                Assert.That((await writer.WriteAsync(0,second,CancellationToken.None)).Succeeded,Is.True);
                var record=slots.Read(0);Assert.That(record.Status,Is.EqualTo(SaveSlotReadStatus.Success));Assert.That(record.Record.ContentJson,Does.Contain("\"value\":2"));
                Assert.That(File.Exists(slots.GetBackupPath(0,1)),Is.True);
                Assert.That(WorldSnapshotCodec.TryRead(record.Record.ContentJson,new Dictionary<string,int>{{"machines",1}},out var loaded,out var reason),Is.True,reason);
                Assert.That(loaded.Revision,Is.EqualTo(2));
            }
            finally
            {
                string resolved=Path.GetFullPath(directory);
                string expectedParent=Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
                if (Path.GetDirectoryName(resolved) != expectedParent || !Path.GetFileName(resolved).StartsWith("AutoEra-B47-",StringComparison.Ordinal)) throw new InvalidOperationException("Unsafe fixture cleanup path.");
                if(Directory.Exists(resolved)) Directory.Delete(resolved,true);
            }
        }
    }
}
