using System;
using AutoEra.Application;
using AutoEra.World;
using AutoEra.World.Region;

namespace AutoEra.UI
{
    /// <summary>
    /// 一次界面打开能接触到的服务句柄。
    ///
    /// 它刻意**不是服务定位器**：只把「打开这个界面时已经存在的东西」交给界面，界面无法
    /// 解析任何未显式传入的服务。这样界面与服务的依赖关系在调用点就是可见的，也不需要
    /// 全局可变状态——EditMode 测试里直接构造即可，不必先造场景。
    ///
    /// 生命周期由框架承担：<c>UIFormBase.OnClose</c> → <c>ReferencePool.Release(Params)</c>
    /// → <c>RefParams.Clear()</c> → <c>VariablePool.ClearVariables(Id)</c>，界面不必自己管。
    /// </summary>
    public sealed class AutoEraUiSession
    {
        private AutoEraUiSession(AutoEraApplicationContext application, AutoEraWorldSession world, InitialRegion region)
        {
            Application = application;
            World = world;
            Region = region;
        }

        /// <summary>应用上下文：存档、场景流与事件发布。</summary>
        public AutoEraApplicationContext Application { get; }

        /// <summary>当前世界会话；主菜单等世界外界面为 null。</summary>
        public AutoEraWorldSession World { get; }

        /// <summary>
        /// 当前现场区域；世界外、区域尚未加载或已经卸载时为 null。
        ///
        /// 区域是**场景级**对象（持有地形、放置体与导航），不属于世界会话，所以它单独作为一个
        /// 可选句柄随会话传递：需要区域数据的界面（放置、世界对象选择、现场资源页）读它，
        /// 不需要的界面忽略它。用完必须判 <see cref="InitialRegion.IsActive"/>——
        /// 会话可能比区域活得久。
        /// </summary>
        public InitialRegion Region { get; }

        /// <summary>当前区域是否可用（非空且未释放）。</summary>
        public bool HasRegion => Region != null && Region.IsActive;

        /// <summary>应用级存档槽服务（世界外界面也要用，例如主菜单的存档入口）。</summary>
        public AutoEra.Save.SaveSlotService SaveSlots => Application?.SaveSlots;

        /// <summary>是否处于一个可用的世界里（现场 HUD 与现场侧栏要求它为 true）。</summary>
        public bool HasWorld => World != null && World.IsActive;

        /// <summary>世界外界面（主菜单、加载、设置等）使用。</summary>
        public static AutoEraUiSession ForApplication(AutoEraApplicationContext application)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new AutoEraUiSession(application, application.ActiveWorldSession, null);
        }

        /// <summary>
        /// 已持有世界会话时使用（流程在进入世界后打开界面）。
        /// 区域随会话一起传递：现场流程在场景就绪后打开界面，此时它已经拿得到区域。
        /// </summary>
        public static AutoEraUiSession ForWorld(AutoEraApplicationContext application, AutoEraWorldSession world,
            InitialRegion region = null)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (world == null || !world.IsActive)
            {
                throw new ArgumentException("A world session must be active.", nameof(world));
            }

            return new AutoEraUiSession(application, world, region);
        }

        /// <summary>把会话写进打开参数；返回同一个 UIParams 便于链式设置其它键。</summary>
        public UIParams WriteTo(UIParams parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            parameters.Set(AutoEraUiParamKeys.Session, this);
            return parameters;
        }
    }
}
