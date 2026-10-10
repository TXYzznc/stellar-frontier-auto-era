using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>
    /// 区域级机器运行时注册表：按机器身份持有 <see cref="RegionMachineRuntime"/>，
    /// 在区域释放时统一销毁。
    ///
    /// 运行服务本身不进存档。普通部署从配置重建；B47正式恢复另从领域快照恢复任务、
    /// 算法与物理责任，不将活动工作重建为空闲，也不发送Startup。
    ///
    /// 导航绑定刻意放在这里而不是 Flow 里：`RegionNavigation.Bind` 的第一个参数就是
    /// `MachineExecutionContext`，所以「运行时创建」必然先于「导航绑定」——
    /// 两者必须在同一个地方按顺序发生，分到两个类里只会制造一个假的依赖方向。
    /// </summary>
    public sealed partial class RegionMachineRuntimeRegistry : IDisposable
    {
        private readonly IReadOnlyDictionary<PersistentId, AutoEra.Logistics.RegionTransferEndpoint> _transferEndpoints;
        private readonly AutoEraWorldSession _session;
        private readonly InitialRegion _region;
        private readonly RegionNavigation _navigation;
        private readonly MachineNavigationSettings _navigationSettings;
        private readonly float _wheelRadius;
        private readonly float _wheelBase;
        private readonly SensorCatalog _sensorCatalog;
        public RegionSensorEnvironment SensorEnvironment { get; }
        public RegionEffectorExecutorRegistry Effectors { get; } = new RegionEffectorExecutorRegistry();
        private readonly Dictionary<PersistentId, RegionMachineRuntime> _runtimes = new Dictionary<PersistentId, RegionMachineRuntime>();
        private readonly List<RegionMachineRuntime> _ordered = new List<RegionMachineRuntime>();
        private readonly List<PersistentId> _pendingDetach = new List<PersistentId>();
        private bool _orderDirty, _advancing;
        private bool _disposed;

        public RegionMachineRuntimeRegistry(
            AutoEraWorldSession session,
            InitialRegion region,
            RegionNavigation navigation,
            MachineNavigationSettings navigationSettings,
            float wheelRadius,
            float wheelBase,
            SensorCatalog sensorCatalog = null,
            IReadOnlyDictionary<PersistentId, AutoEra.Logistics.RegionTransferEndpoint> transferEndpoints = null)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _region = region ?? throw new ArgumentNullException(nameof(region));
            _navigation = navigation;
            _navigationSettings = navigationSettings;
            _wheelRadius = wheelRadius;
            _wheelBase = wheelBase;
            _sensorCatalog = sensorCatalog;
            _transferEndpoints = transferEndpoints;
            SensorEnvironment = new RegionSensorEnvironment(region, true);
        }

        /// <summary>已建立的运行时数量。</summary>
        public int Count => _runtimes.Count;

        /// <summary>区域导航；可能为 null（场景未配置导航地面）。</summary>
        public RegionNavigation Navigation => _navigation;

        public IEnumerable<RegionMachineRuntime> Runtimes => _runtimes.Values;
        public event Action<RegionMachineRuntime> RuntimeAdded,RuntimeRemoved;

        public bool TryGet(PersistentId id, out RegionMachineRuntime runtime) => _runtimes.TryGetValue(id, out runtime);

        /// <summary>先同步业务时刻，随后能源/导航通知才能记录本步时间；不推进算法。</summary>
        public void SetWorldTime(long worldMilliseconds)
        {
            if (_disposed) return;
            foreach (var runtime in _runtimes.Values)
            { runtime.Adapter.SetWorldTime(worldMilliseconds); runtime.Hardware?.SetWorldTime(worldMilliseconds); }
        }

        /// <summary>
        /// 为已部署机器建立运行时。同一台机器重复调用是幂等的（返回已有运行时）。
        ///
        /// `view` 可以为 null：那样仍然建立运行时（算力与传感器与视图无关），
        /// 只是可移动机器拿不到导航并把原因记在
        /// <see cref="RegionMachineRuntime.NavigationUnavailableReason"/> 上。
        /// </summary>
        public bool TryAttach(MachineInstance machine, GameObject view, out RegionMachineRuntime runtime, out string reason)
        {
            runtime = null;
            reason = null;
            if (_disposed) { reason = "区域运行时已被释放"; return false; }
            if (machine == null) { reason = "机器无效"; return false; }
            if (_region == null || !_region.IsActive) { reason = "区域不可用"; return false; }
            if (_runtimes.TryGetValue(machine.Id, out RegionMachineRuntime existing)) { runtime = existing; return true; }

            var context = new MachineExecutionContext(machine, _session.IdAllocator, _session.Events, _session.Resources.GetCargo(machine));

            RegionMachineNavigationBinding binding = null;
            MachineNavigation navigation = null;
            string degradation = null;
            if (machine.Definition.CanMove)
            {
                // 三类「本可移动但拿不到导航」各有各的原因，必须分别说清楚：
                // 没有视图、导航面未就绪、以及 Bind 自己的前置（缺 MotionRig、位置不匹配、已绑定）。
                if (view == null) degradation = "没有实体视图，无法绑定导航";
                else if (_navigation == null) degradation = "该区域未配置导航地面";
                else if (!_navigation.IsReady) degradation = "导航面尚未就绪";
                else
                {
                    try
                    {
                        binding = _navigation.Bind(context, view, _navigationSettings, _wheelRadius, _wheelBase);
                        navigation = binding.Navigation;
                    }
                    catch (Exception error)
                    {
                        // Bind 的前置很严（同身份已部署、位置误差 ≤0.01、子节点有 MotionRig、未重复绑定）。
                        // 这些在生产里都可能不满足，所以它是**降级**而不是异常：
                        // 机器保持已部署、运行时照常建立，只是不能自主移动。
                        binding = null;
                        navigation = null;
                        degradation = "导航绑定失败：" + error.Message;
                    }
                }
            }

            var adapter = new AlgorithmMachineAdapter(context, navigation, _region, _transferEndpoints);
            var hardware = new MachineHardwareRevision(machine);
            var instances = new AlgorithmInstanceService(
                _session.IdAllocator,
                context.Compute,
                hardware.Read,
                adapter.ValidateBindings,
                () => adapter.IsSafe);
            runtime = new RegionMachineRuntime(machine, context, binding, navigation, adapter, instances, hardware)
            {
                NavigationUnavailableReason = degradation,
                PersistentTransport = _session.Resources.Transport,
            };
            runtime.Hardware = new RegionHardwareRuntime(context, _region, SensorEnvironment, _sensorCatalog, Effectors, adapter, view);
            _runtimes.Add(machine.Id, runtime);
            _orderDirty = true;
            RuntimeAdded?.Invoke(runtime);
            return true;
        }

        /// <summary>唯一算法业务步。导航仍只由区域导航推进；回调改变注册表在步末落实。</summary>
        public void AdvanceWorldStep(long worldMilliseconds, double navigationSeconds)
        {
            if (_disposed || _advancing) return;
            if (_orderDirty)
            {
                _ordered.Clear();
                foreach (var runtime in _runtimes.Values) _ordered.Add(runtime);
                _ordered.Sort((a, b) => a.MachineId.CompareTo(b.MachineId));
                _orderDirty = false;
            }
            _advancing = true;
            try
            {
                for (int i = 0; i < _ordered.Count && !_disposed; i++)
                    _ordered[i].AdvanceWorldStep(worldMilliseconds, navigationSeconds);
            }
            finally
            {
                _advancing = false;
                if (_disposed) ReleaseRuntimes();
                else
                {
                    for (int i = 0; i < _pendingDetach.Count; i++) Detach(_pendingDetach[i]);
                    _pendingDetach.Clear();
                }
            }
        }

        /// <summary>撤掉一台机器的运行时（例如机器被撤收）。</summary>
        public bool Detach(PersistentId id)
        {
            if (!_runtimes.TryGetValue(id, out RegionMachineRuntime runtime)) return false;
            if (_advancing)
            {
                if (!_pendingDetach.Contains(id)) _pendingDetach.Add(id);
                return true;
            }
            _runtimes.Remove(id);
            _orderDirty = true;
            RuntimeRemoved?.Invoke(runtime);
            runtime.Dispose();
            return true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_advancing) return;
            ReleaseRuntimes();
        }

        private void ReleaseRuntimes()
        {
            foreach (RegionMachineRuntime runtime in _runtimes.Values) {RuntimeRemoved?.Invoke(runtime);runtime.Dispose();}
            _runtimes.Clear();
            _ordered.Clear();
            _pendingDetach.Clear();
            SensorEnvironment.Dispose();
            RuntimeAdded=null;RuntimeRemoved=null;
        }
    }
}
