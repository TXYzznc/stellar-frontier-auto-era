using System;
using System.Collections.Generic;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.World.Region
{
    /// <summary>
    /// 一个区域的电网：把场景里声明过的能源设施与**已部署的机器**接进同一个结算。
    ///
    /// 规格（06-能源储存与物流「区域级电网」）：每个区域拥有自己的区域电网，
    /// 区域内符合供电条件的机器、建筑、发电设施与储能设施参与同一个供需结算；
    /// 一个区域的富余电力不会自动补充另一个区域。所以这个服务**由区域持有**，
    /// 不是全局单例——否则「前线电网独立运行」这条规则立刻就不成立了。
    ///
    /// **没有设施就不建电网**：场景里一台发电设施都没声明时，
    /// <see cref="HasSupply"/> 为 false、<see cref="Tick"/> 什么都不做，
    /// 机器的供电状态维持原样。这条规则让「接线」不会顺手把还没接内容的世界改成一片漆黑。
    ///
    /// 机器作为负载的规则在 <see cref="MachineEnergyConsumer"/>：请求功率由机器按部件逐项求和，
    /// 本服务只负责「谁参与、按什么顺序结算、结论怎么落回机器」。
    /// </summary>
    public sealed partial class RegionEnergyService : IDisposable
    {
        /// <summary>
        /// 第一版给机器／建筑的默认供电优先级。
        ///
        /// 规格说「系统提供默认优先级，避免玩家在初期承担额外配置负担」，但没有逐个对象列出；
        /// 这里取「普通生产」——中枢与信号塔之类的关键设备属于另一档，装饰性对象才是可暂停。
        /// 将来接供电优先级配置时，这里就是唯一的落点。
        /// </summary>
        public const PowerPriority DefaultMachinePriority = PowerPriority.Production;

        private EnergyGrid _grid = new EnergyGrid();
        private readonly List<RegionEnergyFacility> _facilities = new List<RegionEnergyFacility>();
        private readonly Dictionary<PersistentId, MachineEnergyConsumer> _machines =
            new Dictionary<PersistentId, MachineEnergyConsumer>();
        private MachineRoster _roster;
        private bool _disposed;

        /// <summary>电网本体；测试与调试用。</summary>
        public EnergyGrid Grid => _grid;

        /// <summary>最近一次结算的快照。</summary>
        public EnergyGridSnapshot Snapshot => _grid.Snapshot;

        /// <summary>本区域是否真的建立了供电能力（至少有一台发电或储能设施）。</summary>
        public bool HasSupply => _grid.Generators.Count > 0 || _grid.Storages.Count > 0;

        /// <summary>参与结算的设施数量。</summary>
        public int FacilityCount => _facilities.Count;

        /// <summary>参与结算的设施（按登记顺序，即场景实体的显示顺序）。界面用它列出设施。</summary>
        public IReadOnlyList<RegionEnergyFacility> Facilities => _facilities;

        /// <summary>参与结算的机器数量。</summary>
        public int MachineCount => _machines.Count;

        /// <summary>按机器身份取它在电网里的结算结果；不在电网里时返回 false。</summary>
        public bool TryGetConsumer(PersistentId machineId, out MachineEnergyConsumer consumer) =>
            _machines.TryGetValue(machineId, out consumer);

        /// <summary>最近一次结算时是否处于日照（界面据此说明环境能源为什么没有出力）。</summary>
        public bool LastDaylight { get; private set; }

        /// <summary>
        /// 登记一台场景里的能源设施。身份取**区域对象身份**（不是 MonoBehaviour 实例），
        /// 因为它才是持久且稳定的那个标识。
        /// </summary>
        public void Register(RegionEnergyFacility facility)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RegionEnergyService));
            if (facility == null) throw new ArgumentNullException(nameof(facility));
            if (!facility.IsInitialized) throw new InvalidOperationException("Initialize the facility with its region object id first.");
            if (_facilities.Contains(facility)) return;

            _facilities.Add(facility);
            if (facility.IsGenerator) _grid.AddGenerator(facility.Generator);
            if (facility.Storage != null) _grid.AddStorage(facility.Storage);
        }

        /// <summary>
        /// 把一台**已部署**的机器接成负载。
        ///
        /// 未部署的机器不参与（它在库里不耗电，也不该占用区域功率）；重复登记是幂等的。
        /// </summary>
        public bool TrackMachine(MachineInstance machine)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RegionEnergyService));
            if (machine == null) throw new ArgumentNullException(nameof(machine));
            if (!machine.Deployed) return false;
            if (_machines.ContainsKey(machine.Id)) return true;

            var consumer = new MachineEnergyConsumer(machine, DefaultMachinePriority);
            _machines.Add(machine.Id, consumer);
            _grid.AddConsumer(consumer);
            return true;
        }

        /// <summary>该机器是否已经在电网里。</summary>
        public bool Tracks(PersistentId machineId) => _machines.ContainsKey(machineId);

        /// <summary>
        /// 结算一段时间。
        ///
        /// <paramref name="elapsedSeconds"/> 小于等于 0 或没有设施时只做状态判定、不推进电量
        /// （打开界面时先看一眼当前供需是正常需求）。
        /// </summary>
        public EnergyGridSnapshot Tick(long worldMilliseconds, float elapsedSeconds)
        {
            if(worldMilliseconds<0 || _lastWorldMilliseconds>worldMilliseconds)throw new ArgumentOutOfRangeException(nameof(worldMilliseconds));
            if (!HasSupply)
            {
                return _grid.Snapshot;
            }

            for (int i = 0; i < _facilities.Count; i++)
            {
                _facilities[i].UpdateEnvironment(worldMilliseconds);
            }

            LastDaylight = DaylightCycle.IsDaylight(worldMilliseconds);
            var settled=_grid.Tick(Math.Max(0f, elapsedSeconds), LastDaylight);
            _lastWorldMilliseconds=worldMilliseconds;return settled;
        }

        /// <summary>
        /// 把结算结论落到机器上。返回**实际发生变化**的机器数量。
        ///
        /// 每个世界节拍都要调：`MachineInstance.UpdateSupply` 自己会跳过没变化的情况，
        /// 所以这里不需要再维护一份「上次结论」的副本。
        /// </summary>
        public int ApplySupply()
        {
            int changed = 0;
            foreach (MachineEnergyConsumer consumer in _machines.Values)
            {
                if (consumer.ApplySupply()) changed++;
            }

            return changed;
        }

        /// <summary>
        /// 按花名册对账：把新部署的机器接进来、把已经不在花名册里（或已回收）的机器摘掉。
        ///
        /// 首次绑定后消费部署/撤收通知。稳定节拍只检查绑定身份，不遍历花名册；
        /// force用于显式一致性检查，不重排已有队列。切换花名册或释放区域时退订旧生命周期。
        /// </summary>
        public void Reconcile(MachineRoster roster, bool force = false)
        {
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            if (_disposed) throw new ObjectDisposedException(nameof(RegionEnergyService));
            if (ReferenceEquals(_roster, roster) && !force) return;
            if (_roster != null && !ReferenceEquals(_roster, roster)) OnRosterDisposed(_roster);
            else UnbindRoster();
            if (roster.IsActive)
            {
                _roster = roster;
                roster.DeploymentChanged += OnDeploymentChanged;
                roster.Disposed += OnRosterDisposed;
            }

            foreach (MachineInstance machine in roster.Machines)
            {
                if (machine.Deployed) TrackMachine(machine);
            }

            if (_machines.Count == 0) return;
            _stale.Clear();
            foreach (KeyValuePair<PersistentId, MachineEnergyConsumer> pair in _machines)
            {
                if (!roster.TryGet(pair.Key, out MachineInstance machine) || !machine.Deployed)
                {
                    _stale.Add(pair.Key);
                }
            }

            for (int i = 0; i < _stale.Count; i++)
            {
                _machines.Remove(_stale[i]);
                _grid.RemoveConsumer(_stale[i]);
            }

            _stale.Clear();
        }

        private readonly List<PersistentId> _stale = new List<PersistentId>();

        private void OnDeploymentChanged(MachineInstance machine)
        {
            if (machine.Deployed) TrackMachine(machine);
            else { _machines.Remove(machine.Id); _grid.RemoveConsumer(machine.Id); }
        }
        private void UnbindRoster()
        {
            if (_roster == null) return;
            _roster.DeploymentChanged -= OnDeploymentChanged; _roster.Disposed -= OnRosterDisposed; _roster = null;
        }
        private void OnRosterDisposed(MachineRoster _)
        {
            UnbindRoster();
            foreach (var id in _machines.Keys) _grid.RemoveConsumer(id);
            _machines.Clear();
        }
        public void Dispose()
        {
            if (_disposed) return;
            OnRosterDisposed(_roster); _facilities.Clear(); _stale.Clear(); _disposed = true;
        }
    }
}
