using System;
using AutoEra.Events;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Time;

namespace AutoEra.Application
{
    /// <summary>
    /// Application-lifetime owner for injectable services and one active world session.
    /// It intentionally provides no global accessor; product procedures receive it through
    /// their controlled procedure context in a later integration step.
    /// </summary>
    public sealed class AutoEraApplicationContext : IDisposable
    {
        private bool _isDisposed;
        private readonly IWorldSnapshotWriter _snapshotWriter;

        public AutoEraApplicationContext(IUtcTimeProvider utcTimeProvider, AutoEraWorldSessionFactory worldSessionFactory,
            IEventPublisher eventPublisher = null, SaveSlotService saveSlots = null)
        {
            UtcTimeProvider = utcTimeProvider ?? throw new ArgumentNullException(nameof(utcTimeProvider));
            WorldSessionFactory = worldSessionFactory ?? throw new ArgumentNullException(nameof(worldSessionFactory));
            EventPublisher = eventPublisher;
            SaveSlots = saveSlots ?? SaveSlotService.CreateDefault();
            _snapshotWriter = new SaveSlotWorldSnapshotWriter(SaveSlots);
            Slots = new WorldSlotFlow(this);
        }

        public IUtcTimeProvider UtcTimeProvider { get; }

        /// <summary>
        /// 应用级存档槽服务：主菜单与存档槽界面在世界之外也要用它，因此挂在应用而不是世界会话上。
        /// 可注入根目录，EditMode 测试即可指向临时目录而不碰真实存档。
        /// </summary>
        public SaveSlotService SaveSlots { get; }

        public AutoEraWorldSessionFactory WorldSessionFactory { get; }
        public IEventPublisher EventPublisher { get; }

        public AutoEraWorldSession ActiveWorldSession { get; private set; }
        public int CurrentSlotIndex { get; private set; } = -1;
        public WorldSaveCoordinator SaveCoordinator { get; private set; }
        public WorldSaveExitController SaveExit { get; private set; }
        public WorldSlotFlow Slots { get; }
        public bool BlocksNewWorldCommands => SaveExit?.BlocksNewCommands==true;
        public bool FreezesWorldSimulation => SaveExit?.FreezesSimulation==true;
        public AutoEraSceneFlow SceneFlow { get; } = new AutoEraSceneFlow();
        public string WorldEntryError { get; set; }

        /// <summary>
        /// 界面请求「回到主菜单」。界面拿不到流程实例，所以把意图放在应用上下文里，
        /// 由世界流程在自己的 OnUpdate 里消费——与本类既有的 WorldEntryError 同一种无状态传递方式。
        /// </summary>
        public bool ReturnToMenuRequested { get; private set; }

        public void RequestReturnToMenu() => ReturnToMenuRequested = true;

        public bool ConsumeReturnToMenuRequest()
        {
            if (!ReturnToMenuRequested)
            {
                return false;
            }

            ReturnToMenuRequested = false;
            return true;
        }

        public bool IsDisposed => _isDisposed;

        public bool TryCreateWorldSession(long initialWorldMilliseconds, out AutoEraWorldSession session)
        {
            session = null;
            if (_isDisposed || ActiveWorldSession != null)
            {
                return false;
            }

            session = WorldSessionFactory.Create(initialWorldMilliseconds, EventPublisher);
            ActiveWorldSession = session;
            return true;
        }

        public void ReleaseActiveWorldSession()
        {
            SaveExit?.Dispose();SaveExit=null;
            SaveCoordinator?.Dispose(); SaveCoordinator = null; CurrentSlotIndex = -1;
            if (ActiveWorldSession == null)
            {
                return;
            }

            ActiveWorldSession.Dispose();
            ActiveWorldSession = null;
        }

        public bool TryAttachWorldSaving(int slotIndex,IWorldSnapshotSource source,long savedRevision=0)
        {
            if(_isDisposed || ActiveWorldSession==null || !ActiveWorldSession.IsActive || SaveCoordinator!=null ||
                !SaveSlotService.IsValidSlotIndex(slotIndex) || source==null || savedRevision<0)return false;
            SaveCoordinator=new WorldSaveCoordinator(slotIndex,source,_snapshotWriter,savedRevision);
            SaveExit=new WorldSaveExitController(SaveCoordinator);
            CurrentSlotIndex=slotIndex;return true;
        }

        /// <summary>Only a completely validated, unpublished candidate can replace the active world.</summary>
        public bool TryCommitRestoredWorld(WorldRestoreCandidate candidate,int slotIndex,IWorldSnapshotSource source)
        {
            if(_isDisposed || candidate==null || !candidate.IsReady || source==null || !SaveSlotService.IsValidSlotIndex(slotIndex) ||
                SaveCoordinator?.IsWriting==true)return false;
            var saving=new WorldSaveCoordinator(slotIndex,source,_snapshotWriter,candidate.Document.Revision);
            if(!candidate.TryCommit()) { saving.Dispose();return false; }
            var oldWorld=ActiveWorldSession;var oldSaving=SaveCoordinator;
            SaveExit?.Dispose();
            candidate.World.Events.ActivatePublisher(EventPublisher);
            ActiveWorldSession=candidate.World;SaveCoordinator=saving;CurrentSlotIndex=slotIndex;
            SaveExit=new WorldSaveExitController(saving);
            oldSaving?.Dispose();oldWorld?.Dispose();return true;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            Slots.Dispose();
            ReleaseActiveWorldSession();
            SceneFlow.Dispose();
        }
    }
}
