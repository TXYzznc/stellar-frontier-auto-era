using System;
using AutoEra.UI;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityGameFramework.Runtime;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>Explicit world-procedure scene entry; does not create a global world or use scene searches.</summary>
    public sealed class InitialRegionScene : MonoBehaviour
    {
        [SerializeField] private Rect _bounds = new Rect(-40, -40, 80, 80);
        [SerializeField] private RegionObjectView[] _objects;
        [SerializeField] private string[] _entityPrefabs;
        [SerializeField] private string _entityGroup = "Default";
        [SerializeField] private MeshFilter _navigationGround;
        [SerializeField] private float _navigationRadius = 1.6f;
        [SerializeField] private float _navigationHeight = 2f;
        // 轮径与轴距是**几何参数**，按机器型号而异。第一版只有轮式载体一种可移动型号，
        // 所以先放在场景上（可见、可在预制体里调）；出现第二种可移动型号时必须与占地一样进数据表，
        // 否则又会变成散落在调用点的常量。
        [SerializeField] private float _wheelRadius = .32f;
        [SerializeField] private float _wheelBase = 1.8f;
        private readonly List<int> _entityIds = new List<int>();
        private readonly Dictionary<int, EntityParams> _pendingEntities = new Dictionary<int, EntityParams>();
        private int _entityVersion;
        private bool _entityEvents;
        private Action<string> _failure;
        private AutoEraWorldSession _session;
        private FieldHudForm _hud;
        private long _lastDisplayedSecond = -1;
        public InitialRegion Region { get; private set; }
        public RegionNavigation Navigation { get; private set; }
        public long WorldMilliseconds => _session != null && _session.IsActive ? _session.Clock.WorldMilliseconds : 0;
        private readonly System.Collections.Generic.List<AutoEra.Machines.Sensors.MachineSensorSet> _sensorSets = new System.Collections.Generic.List<AutoEra.Machines.Sensors.MachineSensorSet>();

        /// <summary>已生成的机器实体：GF 实体序号 → 机器身份。用于去重、查找与释放。</summary>
        private readonly Dictionary<int, PersistentId> _machineEntities = new Dictionary<int, PersistentId>();

        /// <summary>区域级机器运行时；区域就绪后建立，释放时统一销毁。</summary>
        private RegionMachineRuntimeRegistry _runtimes;

        /// <summary>
        /// 区域级电网；**场景里声明了能源设施才会建立**（见 <see cref="AttachEnergyFacility"/>）。
        /// 没有设施的区域不建电网，机器的供电状态维持原样。
        /// </summary>
        private RegionEnergyService _energy;

        /// <summary>离散能源事件的识别器；区域就绪后建立，释放时清空（见 <see cref="RecordEnergyEvents"/>）。</summary>
        private AutoEra.Energy.EnergyEventRecorder _energyRecorder;
        private readonly List<AutoEra.Energy.EnergyDiscreteEvent> _energyEvents =
            new List<AutoEra.Energy.EnergyDiscreteEvent>(8);

        /// <summary>区域警报账本与它的监视器；区域就绪后建立（见 <see cref="InitializeNavigation"/>）。</summary>
        private AutoEra.Alerts.AutoEraAlertService _alerts;
        private AutoEra.Alerts.RegionAlertMonitor _alertMonitor;
        private readonly List<AutoEra.Alerts.AlertTransition> _alertTransitions =
            new List<AutoEra.Alerts.AlertTransition>(8);

        /// <summary>区域级机器运行时注册表；区域未就绪时为 null。测试与调试用。</summary>
        public RegionMachineRuntimeRegistry MachineRuntimes => _runtimes;

        /// <summary>区域级电网；本区域没有能源设施时为 null。测试与调试用。</summary>
        public RegionEnergyService Energy => _energy;

        /// <summary>区域警报账本；区域就绪后建立。测试与调试用。</summary>
        public AutoEra.Alerts.AutoEraAlertService Alerts => _alerts;
        public void AttachSensors(AutoEra.Machines.Sensors.MachineSensorSet sensors)
        {
            if (Region == null || !Region.IsActive || sensors == null || _sensorSets.Contains(sensors))
                throw new ArgumentException("An active region and unique explicit sensor set are required.");
            _sensorSets.Add(sensors);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private double _developmentTimeMultiplier = 1d;
        public bool TrySetDevelopmentTimeMultiplier(double multiplier)
        {
            if (double.IsNaN(multiplier) || double.IsInfinity(multiplier) || multiplier < 0d) return false;
            _developmentTimeMultiplier = multiplier;
            return true;
        }
#endif

        public void InitializeRuntime(AutoEraWorldSession session, Action ready, Action<string> failed)
        {
            if (Region != null || session == null || !session.IsActive) throw new InvalidOperationException("Invalid region initialization.");
            if (_objects == null || _entityPrefabs == null || _objects.Length != _entityPrefabs.Length || _objects.Length == 0)
                throw new InvalidOperationException("Region entity configuration missing.");
            _session = session;
            Region = new InitialRegion(session, _bounds);
            int version = ++_entityVersion;
            _failure = failed;
            GF.Event.Subscribe(ShowEntityFailureEventArgs.EventId, OnEntityFailure);
            _entityEvents = true;
            int remaining = _objects.Length;
            try
            {
                for (int i = 0; i < _objects.Length; i++)
                {
                    RegionObjectView seed = _objects[i];
                    if (seed == null || string.IsNullOrWhiteSpace(_entityPrefabs[i])) throw new InvalidOperationException("Missing region seed.");
                    seed.gameObject.SetActive(false);
                    var parameters = EntityParams.Create(seed.transform.position, seed.transform.eulerAngles);
                    int id = parameters.Id;
                    _entityIds.Add(id);
                    _pendingEntities.Add(id, parameters);
                    parameters.OnShowCallback = logic =>
                    {
                        _pendingEntities.Remove(id);
                        if (version != _entityVersion || Region == null) { GF.Entity.HideEntitySafe(id); return; }
                        try
                        {
                            ((InitialRegionEntity)logic).Bind(Region);
                            AttachEnergyFacility((InitialRegionEntity)logic);
                            if (--remaining == 0) { InitializeNavigation(); ready?.Invoke(); }
                        }
                        catch (Exception error) { _failure?.Invoke(error.Message); }
                    };
                    GF.Entity.ShowEntity<InitialRegionEntity>(_entityPrefabs[i], _entityGroup, id, parameters);
                }
            }
            catch { Release(); throw; }
        }

        private void OnEntityFailure(object sender, GameEventArgs args)
        {
            var data = (ShowEntityFailureEventArgs)args;
            if (!_pendingEntities.TryGetValue(data.EntityId, out EntityParams parameters)) return;
            _pendingEntities.Remove(data.EntityId);
            ReferencePool.Release(parameters);
            _failure?.Invoke(data.ErrorMessage);
        }

        public void Initialize(AutoEraWorldSession session)
        {
            if (Region != null) throw new InvalidOperationException("Region scene is already initialized.");
            if (session == null || !session.IsActive) throw new ArgumentException("An active world session is required.");
            _session = session;
            Region = new InitialRegion(session, _bounds);
            try
            {
                if (_objects == null || _objects.Length == 0) throw new InvalidOperationException("Region content is not configured.");
                foreach (RegionObjectView view in _objects)
                {
                    if (view == null) throw new InvalidOperationException("Region content has a missing view.");
                    view.Initialize(Region);
                }
                InitializeNavigation();
            }
            catch { Release(); throw; }
        }

        public void BindHud(FieldHudForm hud)
        {
            if (Region == null) throw new InvalidOperationException("Initialize region before binding HUD.");
            if (_hud != null) _hud.BindRegion(null);
            _hud = hud;
            _lastDisplayedSecond = -1;
            if (_hud != null) { _hud.BindRegion(Region); }
        }

        private void InitializeNavigation()
        {
            if (_navigationGround != null)
            {
                Navigation = new RegionNavigation(Region, _navigationGround, _navigationRadius, _navigationHeight);
                if (!Navigation.RebuildIfNeeded()) throw new InvalidOperationException(Navigation.Error);
            }

            // 运行时注册表即使没有导航地面也要建立：不可移动机器与「导航面未就绪」的机器
            // 同样需要算力池、任务队列与传感器，导航只是其中一项能力。
            _runtimes?.Dispose();
            _runtimes = new RegionMachineRuntimeRegistry(
                _session, Region, Navigation, new AutoEra.Machines.MachineNavigationSettings(), _wheelRadius, _wheelBase);

            // 警报账本与区域同寿命：它记的是**本区域**的问题，跨区域沿用会把 A 区的故障算到 B 区头上。
            _alerts?.Clear();
            _alerts = new AutoEra.Alerts.AutoEraAlertService();
            _alertMonitor = new AutoEra.Alerts.RegionAlertMonitor(_alerts);
        }

        // ------------------------------------------------------- 区域电网

        /// <summary>
        /// 如果这个场景对象声明了能源设施，就把它接进本区域的电网。
        ///
        /// **第一个设施出现时才建立电网**：没有发电设施的区域不该凭空拥有一套供电规则
        /// （那会让「接线」顺手把还没接内容的世界改成一片漆黑）。
        /// 身份取**区域对象身份**而不是场景对象：它才是持久且稳定的那个标识。
        /// </summary>
        private void AttachEnergyFacility(InitialRegionEntity entity)
        {
            RegionEnergyFacility facility = entity.View.GetComponent<RegionEnergyFacility>();
            if (facility == null || facility.ObjectId.IsValid) return;
            if (entity.View.Model == null) return;

            facility.Initialize(entity.View.Model.Id);
            _energy ??= new RegionEnergyService();
            _energy.Register(facility);
        }

        // ------------------------------------------------------- 机器运行时

        /// <summary>
        /// 为已部署机器建立运行时（执行上下文 ＋ 可空导航 ＋ 算法适配器 ＋ 算法实例服务）。
        ///
        /// 可以**没有实体视图**时调用：那样仍会建立运行时，只是可移动机器拿不到导航，
        /// 原因记在 <see cref="RegionMachineRuntime.NavigationUnavailableReason"/> 上。
        /// 领域部署是权威，运行时是派生——所以这里失败只说明「没有运作形态」，不回滚部署。
        /// </summary>
        public bool TryAttachMachineRuntime(PersistentId id, out string reason)
        {
            reason = null;
            if (Region == null || !Region.IsActive) { reason = "区域不可用"; return false; }
            if (_runtimes == null) { reason = "区域导航尚未初始化"; return false; }
            if (_session == null || !_session.Machines.TryGet(id, out AutoEra.Machines.MachineInstance machine))
                { reason = "机器不在花名册里"; return false; }
            if (!Region.TryGet(id, out _))
                { reason = "该机器尚未部署到本区域，不能建立运行时"; return false; }

            RegionObjectView view = FindMachineView(id);
            return _runtimes.TryAttach(machine, view != null ? view.gameObject : null, out _, out reason);
        }

        // ------------------------------------------------------- 机器实体与视图

        /// <summary>
        /// 部署成功后生成机器实体，并把视图**绑定**到已部署的区域对象。
        ///
        /// 返回 false 只表示「没有表现」，**不表示部署失败**：区域对象与花名册状态在
        /// `DeployMachine` 成功时就已成事实，视图只是它的呈现（design.md D2／D5）。
        /// 所以调用方应当把 reason 当作可展示的降级说明，而不是回滚部署。
        ///
        /// 预制体名取自机器定义（`MachineDefinition.Prefab`），路径由框架的
        /// `UtilityBuiltin.GetEntityPath` 规则解析——不在这里硬编码目录。
        /// </summary>
        public bool TrySpawnMachine(PersistentId id, out string reason)
        {
            reason = null;
            if (Region == null || !Region.IsActive) { reason = "区域不可用"; return false; }
            if (_session == null || !_session.Machines.TryGet(id, out AutoEra.Machines.MachineInstance machine))
                { reason = "机器不在花名册里"; return false; }
            if (!Region.TryGet(id, out RegionObject model))
                { reason = "该机器尚未部署到本区域，不能先生成实体"; return false; }
            if (!machine.Definition.HasPrefab)
                { reason = "该型号未配置实体预制体"; return false; }
            if (HasMachineEntity(id))
                { reason = "该机器已经有实体"; return false; }

            int version = _entityVersion;
            var parameters = EntityParams.Create(
                new Vector3(model.Position.x, 0f, model.Position.y),
                new Vector3(0f, model.Yaw, 0f));
            int entityId = parameters.Id;
            _machineEntities.Add(entityId, id);
            parameters.OnShowCallback = logic =>
            {
                if (version != _entityVersion || Region == null || !Region.IsActive)
                {
                    GF.Entity.HideEntitySafe(entityId);
                    return;
                }

                try
                {
                    ((InitialRegionMachineEntity)logic).Bind(Region, id);
                    // 视图就绪后紧接着建立运行时：导航绑定需要实体 GameObject，
                    // 而运行时又必须先于导航绑定存在，所以两件事必须在这一个回调里按顺序发生。
                    TryAttachMachineRuntime(id, out _);
                }
                catch (Exception error)
                {
                    // 视图绑定失败同样是「没有表现」而不是部署失败：留下可查的记录，不回滚领域。
                    Debug.LogError($"[AutoEra][Region] 机器实体绑定视图失败 id={id.Value}：{error.Message}");
                }
            };
            GF.Entity.ShowEntity<InitialRegionMachineEntity>(machine.Definition.Prefab, _entityGroup, entityId, parameters);
            return true;
        }

        /// <summary>该机器是否已经生成过实体。</summary>
        public bool HasMachineEntity(PersistentId id)
        {
            foreach (PersistentId existing in _machineEntities.Values)
            {
                if (existing == id) return true;
            }

            return false;
        }

        /// <summary>已生成的机器实体数量。测试与调试用。</summary>
        public int MachineEntityCount => _machineEntities.Count;

        /// <summary>取某台机器的视图；没有实体或尚未绑定完成时返回 null。测试与调试用。</summary>
        public RegionObjectView FindMachineView(PersistentId id)
        {
            foreach (KeyValuePair<int, PersistentId> pair in _machineEntities)
            {
                if (pair.Value != id || !GF.Entity.HasEntity(pair.Key)) continue;
                if (GF.Entity.GetEntity(pair.Key).Logic is InitialRegionMachineEntity logic) return logic.View;
            }

            return null;
        }

        private void ReleaseMachineEntities()
        {
            foreach (int entityId in _machineEntities.Keys) GF.Entity.HideEntitySafe(entityId);
            _machineEntities.Clear();
        }

        public bool Select(RegionObjectView view, bool inputBlocked)
            => Region != null && Region.Select(view != null && view.Model != null ? view.Model.Id : PersistentId.Invalid, inputBlocked);
        public void ShowFieldAccess(bool accessible, bool managementOpen) => _hud?.SetFieldAccess(accessible, managementOpen);

        public void Advance(double realSeconds)
        {
            if (_session == null || !_session.IsActive) { Release(); return; }
            Navigation?.Tick(UnityEngine.Time.realtimeSinceStartupAsDouble, (float)realSeconds);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            bool advanced = _session.Clock.TryAdvanceDevelopmentRealtimeSeconds(realSeconds, _developmentTimeMultiplier);
#else
            bool advanced = _session.Clock.TryAdvanceRealtimeSeconds(realSeconds);
#endif
            if (!advanced)
                throw new ArgumentOutOfRangeException(nameof(realSeconds));
            long second = _session.Clock.WorldMilliseconds / 1000;
            long sensorTime = _session.Clock.WorldMilliseconds;
            int sensorCount = _sensorSets.Count;
            for (int i = 0; i < sensorCount && i < _sensorSets.Count; i++) _sensorSets[i].Tick(sensorTime);
            AdvanceEnergy(_session.Clock.WorldMilliseconds, realSeconds);
            AdvanceAlerts();
            if (_hud != null && second != _lastDisplayedSecond)
            {
                _lastDisplayedSecond = second;
                _hud.ShowWorldTime(_session.Clock.WorldMilliseconds);
            }
        }

        /// <summary>
        /// 推进区域电网：先按花名册对账（新部署的机器接进来、离开的摘掉），
        /// 再按真实经过时间结算，最后把结论落回机器。
        ///
        /// 顺序不能颠倒：先结算再对账会让本帧新部署的机器白等一个节拍才通电，
        /// 而先落结论再对账则会让离开的机器带着旧结论留在电网里。
        /// </summary>
        private void AdvanceEnergy(long worldMilliseconds, double realSeconds)
        {
            if (_energy == null) return;
            _energy.Reconcile(_session.Machines);
            _energy.Tick(worldMilliseconds, (float)realSeconds);
            _energy.ApplySupply();
            RecordEnergyEvents();
        }

        /// <summary>
        /// 把这一次结算产生的**离散**事件写进事件日志（规格 06「第一版能源界面」：
        /// 能源历史只记离散事件，不做连续功率曲线）。
        ///
        /// 顺序是「结算 → 落结论 → 记账」：记录描述的是这一次结算的结论，
        /// 先记账再结算会记下上一帧的状态。没有事件服务的世界（单元测试、离线推演）
        /// 照常推进电网，只是不记账——记账不该成为结算的前置条件。
        /// </summary>
        private void RecordEnergyEvents()
        {
            if (_session == null || !_session.IsActive) return;
            AutoEra.Events.AutoEraEventService events = _session.Events;
            if (events == null) return;

            _energyRecorder ??= new AutoEra.Energy.EnergyEventRecorder();
            _energyEvents.Clear();
            _energyRecorder.Capture(_energy.Snapshot, _energy.Facilities, _energyEvents);

            for (int i = 0; i < _energyEvents.Count; i++)
            {
                AutoEra.Energy.EnergyDiscreteEvent item = _energyEvents[i];
                events.PublishNotice(AutoEra.Events.EventDomain.Energy, item.Subject,
                    AutoEra.Energy.EnergyEventText.Label(item.Kind));
            }
        }

        /// <summary>
        /// 把这一次结算之后的真实状态交给警报账本，并把**真实的跨越**写进事件日志。
        ///
        /// 顺序：结算 → 落结论 → 记账（能源事件）→ 报警（警报账本）。两者读同一份快照，
        /// 但一个记流水（发生了什么）、一个记台账（现在有什么问题）；所以警报不是从事件推出来的，
        /// 而是每帧直接读真值——漏一条消息不会让警报停在错误状态。
        /// </summary>
        private void AdvanceAlerts()
        {
            if (_alerts == null || _alertMonitor == null || _session == null || !_session.IsActive) return;

            _alertTransitions.Clear();
            _alertMonitor.Capture(_session.Machines, _energy, _session.Clock.WorldMilliseconds, _alertTransitions);
            if (_alertTransitions.Count == 0) return;

            AutoEra.Events.AutoEraEventService events = _session.Events;
            if (events == null) return;

            for (int i = 0; i < _alertTransitions.Count; i++)
            {
                AutoEra.Alerts.AlertTransition item = _alertTransitions[i];
                events.PublishNotice(AutoEra.Events.EventDomain.Alert, item.Source,
                    AutoEra.Alerts.AlertCatalog.Label(item.Kind) + (item.Raised ? "·出现" : "·恢复"));
            }
        }

        /// <summary>
        /// 事件主体的显示名：机器取花名册里的名字，设施取区域对象名。
        /// 取不到就如实返回 null——界面会说「对象已不在」，而不是编一个名字。
        /// </summary>
        public string ResolveObjectName(PersistentId id)
        {
            if (!id.IsValid) return null;
            if (_session != null && _session.IsActive &&
                _session.Machines.TryGet(id, out AutoEra.Machines.MachineInstance machine))
            {
                return machine.Name;
            }

            if (Region != null && Region.TryGet(id, out RegionObject model) && !string.IsNullOrEmpty(model.Name))
            {
                return model.Name;
            }

            return null;
        }

        public void Release()
        {
            foreach (var sensors in _sensorSets) sensors.Dispose();
            _sensorSets.Clear();
            // 先回收机器实体（它们的视图要退订），再拆运行时，最后放导航。
            ReleaseMachineEntities();
            _runtimes?.Dispose();
            _runtimes = null;
            _energy = null;
            _energyRecorder?.Reset();
            _energyRecorder = null;
            _energyEvents.Clear();
            _alerts?.Clear();
            _alerts = null;
            _alertMonitor = null;
            _alertTransitions.Clear();
            Navigation?.Dispose(); Navigation = null;
            _entityVersion++;
            _failure = null;
            if (_entityEvents) { GF.Event.Unsubscribe(ShowEntityFailureEventArgs.EventId, OnEntityFailure); _entityEvents = false; }
            foreach (int id in _entityIds) GF.Entity.HideEntitySafe(id);
            _entityIds.Clear();
            foreach (EntityParams parameters in _pendingEntities.Values) ReferencePool.Release(parameters);
            _pendingEntities.Clear();
            if (_hud != null) _hud.BindRegion(null);
            _hud = null;
            if (_objects != null) foreach (RegionObjectView view in _objects) if (view != null) view.Release();
            Region?.Dispose();
            Region = null;
            _session = null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _developmentTimeMultiplier = 1d;
#endif
        }

        private void OnDestroy() => Release();
    }
}
