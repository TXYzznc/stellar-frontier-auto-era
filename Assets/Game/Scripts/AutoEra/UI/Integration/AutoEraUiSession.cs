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
        private AutoEraUiSession(AutoEraApplicationContext application, AutoEraWorldSession world, InitialRegion region,
            AutoEra.Input.RegionInputModule regionInput, RegionMachineRuntimeRegistry machineRuntimes,
            RegionEnergyService regionEnergy, AutoEra.Alerts.AutoEraAlertService regionAlerts)
        {
            Application = application;
            World = world;
            Region = region;
            RegionInput = regionInput;
            MachineRuntimes = machineRuntimes;
            RegionEnergy = regionEnergy;
            RegionAlerts = regionAlerts;
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

        /// <summary>
        /// 现场区域的世界输入模块；现场外或场景未配置时为 null。
        ///
        /// 世界放置界面需要它，理由是**放置预览的归属**：指针→区域坐标的换算、朝向旋转、
        /// 点击提交都由该模块承担（`RegionInputModule` 已经拥有 `RegionPlacementPreview` 的
        /// 完整交互），界面只负责把流程交给它并读回 `IsValid`/`Reason`/`LastOutcome`。
        /// 如果界面自己再实现一套预览交互，世界里就会出现两条互相竞争的预览。
        /// </summary>
        public AutoEra.Input.RegionInputModule RegionInput { get; }

        /// <summary>当前区域是否具备可驱动落位预览的输入模块。</summary>
        public bool HasRegionInput => RegionInput != null;

        /// <summary>
        /// 区域级机器运行时注册表；区域未就绪或场景未建立时（尚未初始化、已释放）为 null。
        ///
        /// 算法界面靠它找「这台机器的执行上下文与算法实例服务」——那也是唯一一处
        /// 生产运行路径会创建它们的地方。没有它，算法域在界面眼里就还是「没有创建者」。
        /// </summary>
        public RegionMachineRuntimeRegistry MachineRuntimes { get; }

        /// <summary>当前区域是否已经建立了机器运行时注册表。</summary>
        public bool HasMachineRuntimes => MachineRuntimes != null;

        /// <summary>
        /// 当前区域的电网；区域里**没有声明任何能源设施**时为 null。
        ///
        /// 它单独作为一个可选句柄：能源界面靠它取供需快照与设施列表，
        /// 而「没有电网」是一句可展示的说明（那个区域确实还没有供电能力），不是错误。
        /// </summary>
        public RegionEnergyService RegionEnergy { get; }

        /// <summary>当前区域是否建立了电网。</summary>
        public bool HasRegionEnergy => RegionEnergy != null;

        /// <summary>
        /// 当前区域的警报账本；区域未就绪时为 null。
        ///
        /// 警报**由区域持有**而不是全局：它记的是本区域的缺电、燃料与损坏，
        /// 跨区域沿用会把 A 区的故障算到 B 区头上。世界外界面（主菜单、加载）看不到任何警报——
        /// 那时确实还没有区域，界面应当说「不在世界里」而不是「没有警报」。
        /// </summary>
        public AutoEra.Alerts.AutoEraAlertService RegionAlerts { get; }

        /// <summary>当前区域是否建立了警报账本。</summary>
        public bool HasRegionAlerts => RegionAlerts != null;

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

            return new AutoEraUiSession(application, application.ActiveWorldSession, null, null, null, null, null);
        }

        /// <summary>
        /// 已持有世界会话时使用（流程在进入世界后打开界面）。
        /// 区域随会话一起传递：现场流程在场景就绪后打开界面，此时它已经拿得到区域。
        /// </summary>
        public static AutoEraUiSession ForWorld(AutoEraApplicationContext application, AutoEraWorldSession world,
            InitialRegion region = null, AutoEra.Input.RegionInputModule regionInput = null,
            RegionMachineRuntimeRegistry machineRuntimes = null, RegionEnergyService regionEnergy = null,
            AutoEra.Alerts.AutoEraAlertService regionAlerts = null)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (world == null || !world.IsActive)
            {
                throw new ArgumentException("A world session must be active.", nameof(world));
            }

            return new AutoEraUiSession(application, world, region, regionInput, machineRuntimes, regionEnergy,
                regionAlerts);
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
