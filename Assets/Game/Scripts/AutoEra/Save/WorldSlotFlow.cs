using System;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Region;
using AutoEra.World.Time;

namespace AutoEra.Save
{
    public interface IWorldProgressSetup
    {
        string NewProgressUnavailableReason { get; }
        IRegionWorldPersistence CreatePersistence();
        MachineDefinition ResolveMachine(int model,int level);
        ComponentDefinition ResolveComponent(int model,int level);
        bool TryInitializeNew(AutoEraWorldSession world,out string reason);
        bool SupportsOfflineContinuation { get; }
        void BeginOfflineContinuation(WorldRestoreCandidate candidate,InitialRegionScene scene,WorldSlotEntryRequest request,Action completed,Action<string> failed);
    }
    public sealed class WorldSlotEntryRequest : IDisposable
    {
        internal WorldSlotEntryRequest(int slot,IWorldProgressSetup setup,IRegionWorldPersistence domains,WorldRestoreCandidate candidate,bool offline,bool resume,DateTimeOffset savedUtc,DateTimeOffset utc)
        { SlotIndex=slot;Setup=setup;Persistence=domains;Candidate=candidate;RequiresOffline=offline;ResumesOfflineCheckpoint=resume;SavedUtc=savedUtc;TargetUtc=utc; }
        public int SlotIndex { get; }
        public IWorldProgressSetup Setup { get; }
        public IRegionWorldPersistence Persistence { get; }
        public WorldRestoreCandidate Candidate { get; }
        public bool IsNew=>Candidate==null;
        public bool RequiresOffline { get; }
        public bool ResumesOfflineCheckpoint { get; }
        public DateTimeOffset SavedUtc { get; }
        public DateTimeOffset TargetUtc { get; }
        public void Dispose()=>Candidate?.Dispose();
    }
    /// <summary>Menu requests prepare isolated candidates. Backups and unfinished offline work have explicit gates.</summary>
    public sealed class WorldSlotFlow : IDisposable
    {
        private readonly AutoEraApplicationContext _context;
        private WorldSlotEntryRequest _pending;
        private bool _disposed;
        public WorldSlotFlow(AutoEraApplicationContext context) { _context=context ?? throw new ArgumentNullException(nameof(context)); }
        public IWorldProgressSetup Setup { get; private set; }
        public bool HasPendingRequest=>_pending!=null;
        public string NewProgressUnavailableReason=>Setup?.NewProgressUnavailableReason ?? "世界进度层尚未接入：读取与新建存档暂不可用。";
        public void Configure(IWorldProgressSetup setup)
        {
            if(_disposed || HasPendingRequest || _context.ActiveWorldSession!=null)throw new InvalidOperationException("Cannot replace world entry setup while a world is owned.");
            Setup=setup ?? throw new ArgumentNullException(nameof(setup));
        }
        public bool CanRequestNew=>!_disposed && !_context.IsDisposed && _context.ActiveWorldSession==null && !HasPendingRequest && Setup!=null && string.IsNullOrEmpty(Setup.NewProgressUnavailableReason);
        public bool CanRequestContinue=>!_disposed && !_context.IsDisposed && _context.ActiveWorldSession==null && !HasPendingRequest && Setup!=null;
        public bool TryRequestNew(int slot,out string reason)
        {
            reason=NewProgressUnavailableReason;
            if(!CanRequestNew || !SaveSlotService.IsValidSlotIndex(slot))return false;
            if(_context.SaveSlots.Read(slot).Status!=SaveSlotReadStatus.Empty || _context.SaveSlots.HasBackup(slot))
            { reason="该槽位已有进度或备份，请先明确删除后再新建。";return false; }
            var domains=Setup.CreatePersistence();if(domains==null) { reason="世界领域保存配置尚未就绪";return false; }
            var utc=_context.UtcTimeProvider.GetUtcNow();
            _pending=new WorldSlotEntryRequest(slot,Setup,domains,null,false,false,utc,utc);reason=null;return true;
        }
        public bool TryRequestContinue(int slot,out string reason)
        {
            reason="当前无法请求继续进度";
            if(!CanRequestContinue || !SaveSlotService.IsValidSlotIndex(slot))return false;
            var result=_context.SaveSlots.Read(slot);
            if(result.IsFromBackup) {reason="主文件不可用，请先确认备份恢复时间和进度损失后恢复。";return false;}
            if(!result.IsPrimary || result.Record==null) {reason="该槽位没有可继续的有效主存档。";return false;}
            var record=result.Record;var targetUtc=_context.UtcTimeProvider.GetUtcNow();
            if(record.SavedUtc==DateTime.MinValue) {reason="存档缺少可信保存时刻，无法确定离线结算范围。";return false;}
            bool offline=record.OfflineSettlementPending || TimeUtil.GetNonNegativeDuration(new DateTimeOffset(record.SavedUtc),targetUtc).Ticks>=TimeSpan.TicksPerMillisecond;
            if(offline && !Setup.SupportsOfflineContinuation) {reason="该进度需要离线结算，结算入口尚未就绪，不能跳过后进入世界。";return false;}
            var domains=Setup.CreatePersistence();
            if(!WorldRestoreTransaction.TryPrepare(record.ContentJson,_context.WorldSessionFactory,domains,Setup.ResolveMachine,Setup.ResolveComponent,out var candidate,out reason))return false;
            if(candidate.Document.WorldMilliseconds!=record.WorldTimeMilliseconds)
            { candidate.Dispose();reason="存档外层世界时刻与完整快照不一致";return false; }
            _pending=new WorldSlotEntryRequest(slot,Setup,domains,candidate,offline,record.OfflineSettlementPending,new DateTimeOffset(record.SavedUtc),targetUtc);reason=null;return true;
        }
        public bool TryConsume(out WorldSlotEntryRequest request)
        { request=_pending;if(request==null)return false;_pending=null;return true; }
        public void Dispose()
        { if(_disposed)return;_disposed=true;_pending?.Dispose();_pending=null;Setup=null; }
    }
}
