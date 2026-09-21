using System.Collections.Generic;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.Alerts
{
    /// <summary>账本的一次状态跨越（用于写事件日志；界面订阅账本本身的变化）。</summary>
    public readonly struct AlertTransition
    {
        public AlertTransition(AlertKind kind, PersistentId source, bool raised)
        {
            Kind = kind;
            Source = source;
            Raised = raised;
        }

        public AlertKind Kind { get; }
        public PersistentId Source { get; }

        /// <summary>true＝问题出现，false＝已恢复。</summary>
        public bool Raised { get; }
    }

    /// <summary>
    /// 把「领域现在的样子」翻译成警报：每个节拍读一次真实状态，只在跨越时让账本变化。
    ///
    /// 它刻意**不订阅任何事件、也不缓存判断结果**：警报的判据是领域当前的真值
    /// （机器是不是被缺电停机、储能是不是 0、燃料是不是没了、完整度是不是 0），
    /// 而不是「谁发过一条消息」。这样即使某一帧的事件没被收到，账本也会在下一帧自己收敛——
    /// 警报不会因为漏了一条消息而永久停在错误状态。
    ///
    /// 与离散能源事件识别器（<see cref="EnergyEventRecorder"/>）的分工：
    /// 那个记的是**发生了什么事**（历史，一次一条），这个说的是**现在有什么问题**（状态，可恢复）。
    /// 两者读同一份快照，但一个是流水、一个是台账。
    /// </summary>
    public sealed class RegionAlertMonitor
    {
        private readonly AutoEraAlertService _alerts;

        public RegionAlertMonitor(AutoEraAlertService alerts)
        {
            _alerts = alerts;
        }

        /// <summary>
        /// 读一遍真实状态并让账本收敛；把本次发生的跨越追加到 <paramref name="into"/>（不清空它）。
        /// </summary>
        public void Capture(MachineRoster machines, RegionEnergyService energy, long worldMilliseconds,
            List<AlertTransition> into)
        {
            CaptureMachines(machines, energy, worldMilliseconds, into);
            CaptureFacilities(energy, worldMilliseconds, into);
        }

        private void CaptureMachines(MachineRoster machines, RegionEnergyService energy, long worldMilliseconds,
            List<AlertTransition> into)
        {
            if (machines == null) return;

            foreach (MachineInstance machine in machines.Machines)
            {
                if (!machine.Deployed) continue;

                // 缺电停机：用「电网因缺电把它停了」而不是「它现在没电」——
                // 后者在刚部署、还没轮到结算时也成立，那会把「还没接上」报成一次事故。
                bool stopped = energy != null && energy.TryGetConsumer(machine.Id, out MachineEnergyConsumer consumer)
                    && consumer.IsStoppedByShortage;
                Track(AlertKind.EnergyShortage, machine.Id, stopped, worldMilliseconds, into);

                Track(AlertKind.MachineDestroyed, machine.Id, machine.Integrity <= 0d, worldMilliseconds, into);
            }
        }

        private void CaptureFacilities(RegionEnergyService energy, long worldMilliseconds, List<AlertTransition> into)
        {
            if (energy == null) return;

            IReadOnlyList<RegionEnergyFacility> facilities = energy.Facilities;
            for (int i = 0; i < facilities.Count; i++)
            {
                RegionEnergyFacility facility = facilities[i];
                if (facility == null) continue;

                if (facility.Storage != null)
                {
                    // 只有区域真的有储能时，电量 0 才是「耗尽」——没有储能的区域恒为 0。
                    bool depleted = facility.Storage.Capacity > 0f && facility.Storage.Charge <= 0f;
                    Track(AlertKind.StorageDepleted, StorageSubject(facilities), depleted, worldMilliseconds, into);
                    continue;
                }

                IEnergyGenerator generator = facility.Generator;
                if (generator == null || generator.Kind != GeneratorKind.Fuel) continue;

                Track(AlertKind.FuelExhausted, facility.ObjectId, facility.RemainingBiomass <= 0f,
                    worldMilliseconds, into);
            }
        }

        /// <summary>问题在就报、不在就销；跨越时才动账本，因此每帧调用不会产生任何写入。</summary>
        private void Track(AlertKind kind, PersistentId source, bool present, long worldMilliseconds,
            List<AlertTransition> into)
        {
            if (present)
            {
                if (_alerts.Raise(kind, source, worldMilliseconds))
                {
                    // 活跃期内重复发生也会走这里，但只有「新开一条」才值得写日志。
                    into?.Add(new AlertTransition(kind, source, raised: true));
                }

                return;
            }

            if (_alerts.Resolve(kind, source, worldMilliseconds))
            {
                into?.Add(new AlertTransition(kind, source, raised: false));
            }
        }

        /// <summary>多台储能设施时没有单一主体，用 Invalid 表示「区域口径」（与能源事件一致）。</summary>
        private static PersistentId StorageSubject(IReadOnlyList<RegionEnergyFacility> facilities)
        {
            PersistentId found = PersistentId.Invalid;
            for (int i = 0; i < facilities.Count; i++)
            {
                if (facilities[i] == null || facilities[i].Storage == null) continue;
                if (found.IsValid) return PersistentId.Invalid;
                found = facilities[i].ObjectId;
            }

            return found;
        }
    }
}
