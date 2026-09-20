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

        public AutoEraApplicationContext(IUtcTimeProvider utcTimeProvider, AutoEraWorldSessionFactory worldSessionFactory,
            IEventPublisher eventPublisher = null, SaveSlotService saveSlots = null)
        {
            UtcTimeProvider = utcTimeProvider ?? throw new ArgumentNullException(nameof(utcTimeProvider));
            WorldSessionFactory = worldSessionFactory ?? throw new ArgumentNullException(nameof(worldSessionFactory));
            EventPublisher = eventPublisher;
            SaveSlots = saveSlots ?? SaveSlotService.CreateDefault();
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
            if (ActiveWorldSession == null)
            {
                return;
            }

            ActiveWorldSession.Dispose();
            ActiveWorldSession = null;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            ReleaseActiveWorldSession();
            SceneFlow.Dispose();
        }
    }
}
