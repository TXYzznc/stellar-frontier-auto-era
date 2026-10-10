using System;
using System.Collections.Generic;
using AutoEra.World.Identity;
using AutoEra.Energy;

// Frozen pre-B46 settlement reference; test-only, never used by production.
namespace AutoEra.Tests.EnergyBench
{
    public sealed class EnergyGridReference
    {
        private const float Epsilon = 1e-4f;

        private readonly List<IEnergyGenerator> _generators = new List<IEnergyGenerator>();
        private readonly List<IEnergyStorage> _storages = new List<IEnergyStorage>();
        private readonly List<IEnergyConsumer> _consumers = new List<IEnergyConsumer>();
        private readonly List<PersistentId> _stopped = new List<PersistentId>();
        private long _queueCounter;

        public IReadOnlyList<IEnergyGenerator> Generators => _generators;
        public IReadOnlyList<IEnergyStorage> Storages => _storages;
        public IReadOnlyList<IEnergyConsumer> Consumers => _consumers;

        /// <summary>最后一次结算的快照；还没结算过时是零值。</summary>
        public EnergyGridSnapshot Snapshot { get; private set; }

        /// <summary>
        /// 登记一台发电设施。**登记顺序就是建造顺序**：多台同类燃料发电站按它依次承担剩余需求。
        /// </summary>
        public void AddGenerator(IEnergyGenerator generator)
        {
            if (generator == null) throw new ArgumentNullException(nameof(generator));
            _generators.Add(generator);
        }

        public void AddStorage(IEnergyStorage storage)
        {
            if (storage == null) throw new ArgumentNullException(nameof(storage));
            _storages.Add(storage);
        }

        /// <summary>
        /// 登记一个用电对象。进入队列的顺序由本方法自动分配（规格：同优先级负载按对象进入供电队列的
        /// 时间停机与恢复），调用方不需要自己维护序号，也就不会出现两处序号不一致。
        /// </summary>
        public void AddConsumer(IEnergyConsumer consumer)
        {
            if (consumer == null) throw new ArgumentNullException(nameof(consumer));
            consumer.QueueOrder = _queueCounter++;
            _consumers.Add(consumer);
        }

        /// <summary>
        /// 移除一个用电对象（机器被回收／移出本区域时）。
        ///
        /// 只从队列中段摘掉那一条，其余参与方的**进入队列顺序不变**——
        /// 那正是同级停机顺序的判据，重建整个队列会让停机顺序变成时间的函数。
        /// </summary>
        public bool RemoveConsumer(PersistentId id)
        {
            for (int i = 0; i < _consumers.Count; i++)
            {
                if (_consumers[i].Id != id) continue;
                _consumers.RemoveAt(i);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 结算一段时间并返回本次快照。
        ///
        /// <paramref name="elapsedSeconds"/> 为 0 时只做一次「当前供需是否成立」的判定、
        /// 改变谁被停机的结论，但**不改变任何电量**——打开界面时要立刻拿到正确状态，
        /// 而「看一眼前状态」不该给电池充电。
        /// </summary>
        public EnergyGridSnapshot Tick(float elapsedSeconds, bool daylight)
        {
            if (elapsedSeconds < 0f) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (float.IsNaN(elapsedSeconds)) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            _stopped.Clear();

            // 上一时刻的停机结论不延续：每次结算都重新判定，
            // 否则一次缺电会让对象永久停机，供电恢复后也回不来。
            for (int i = 0; i < _consumers.Count; i++)
            {
                IEnergyConsumer consumer = _consumers[i];
                consumer.IsStoppedByShortage = false;
            }

            Plan plan;
            while (true)
            {
                plan = BuildPlan(daylight, elapsedSeconds);
                if (plan.Shortfall <= 0f)
                {
                    break;
                }

                IEnergyConsumer victim = NextVictim();
                if (victim == null)
                {
                    // 没有可停的对象却仍有缺口：如实报告，不假装供电成立。
                    // （按当前契约这不可达——任何请求用电的对象都可被停机——但保留这条分支，
                    //  将来若出现「不可停机的关键负载」，它会给出正确的答案而不是无穷循环。）
                    Apply(elapsedSeconds, plan);
                    return Snapshot = Build(plan, forceShortfall: true);
                }

                victim.IsStoppedByShortage = true;
                _stopped.Add(victim.Id);
            }

            Apply(elapsedSeconds, plan);
            return Snapshot = Build(plan, forceShortfall: false);
        }

        /// <summary>停机顺序：优先级最低的先停；同级按进入供电队列的时间**后进先停**。</summary>
        private IEnergyConsumer NextVictim()
        {
            IEnergyConsumer worst = null;
            for (int i = 0; i < _consumers.Count; i++)
            {
                IEnergyConsumer consumer = _consumers[i];
                // 不请求用电的对象没有负载可停：把它标成「因缺电停机」是错的，
                // 界面会因此把「玩家自己关掉的设备」显示成故障。
                if (consumer.IsStoppedByShortage || !consumer.IsDemandActive) continue;
                if (worst == null) { worst = consumer; continue; }
                if (consumer.Priority < worst.Priority) { worst = consumer; continue; }
                if (consumer.Priority == worst.Priority && consumer.QueueOrder > worst.QueueOrder) worst = consumer;
            }

            return worst;
        }

        private sealed class Plan
        {
            public float Load;
            public float Generated;
            public float Discarded;
            public float StorageNetPower; // 正 = 充电，负 = 放电
            public float Shortfall;
        }

        /// <summary>
        /// 按固定供电顺序算一次供需：免费环境能源 → 燃料发电 → 燃料充电（负载已满足时）→ 蓄电池。
        ///
        /// 方法名刻意不叫 `Plan`：C# 不允许同名的方法与嵌套类型共存，
        /// 而那个内部类叫 `Plan` 更贴切——所以是方法让路。
        /// </summary>
        private Plan BuildPlan(bool daylight, float elapsedSeconds)
        {
            var plan = new Plan();

            // ① 负载：只统计请求用电的对象。被缺电停机的对象耗电归零。
            //    功率由对象自己按部件状态求和给出，电网不替它选「待机还是稳定」。
            for (int i = 0; i < _consumers.Count; i++)
            {
                IEnergyConsumer consumer = _consumers[i];
                if (!consumer.IsDemandActive || consumer.IsStoppedByShortage) continue;
                plan.Load += Math.Max(0f, consumer.RequestedPower);
            }

            // ② 免费环境能源优先直接满足负载；盈余自动充电、储满后舍弃。
            float environment = 0f;
            for (int i = 0; i < _generators.Count; i++)
            {
                IEnergyGenerator generator = _generators[i];
                if (generator.Kind != GeneratorKind.Environment || !generator.IsOn) continue;
                environment += Math.Max(0f, generator.EnvironmentPower);
            }

            plan.Generated = environment;
            float remaining = plan.Load - environment;

            // ③ 已开启的燃料发电站只补足剩余需求，按建造顺序依次承担，且不超过额定。
            for (int i = 0; i < _generators.Count; i++)
            {
                IEnergyGenerator generator = _generators[i];
                if (generator.Kind != GeneratorKind.Fuel) continue;
                if (!generator.IsOn || remaining <= 0f) { generator.ActualOutputPower = 0f; continue; }

                float output = Math.Min(generator.RatedPower, remaining);
                output = Math.Min(output, PowerFromFuel(generator, elapsedSeconds));
                generator.ActualOutputPower = output;
                plan.Generated += output;
                remaining -= output;
            }

            // ④ 燃料充电：**实时负载优先于任何充电需求**，所以只在负载已满足时才考虑；
            //    并且要玩家开了许可、且储能未达到目标比例。达到目标后自动回到只满足实时负载。
            if (remaining <= 0f)
            {
                plan.Generated += AllocateChargingFromFuel(elapsedSeconds);
            }

            // ⑤ 蓄电池：缺口放电、盈余充电；**同一时刻只能选一边**。
            float gap = plan.Load - plan.Generated;
            if (gap > Epsilon)
            {
                float discharge = Math.Min(gap, AvailableDischargePower(elapsedSeconds));
                plan.StorageNetPower = -discharge;
                plan.Shortfall = gap - discharge;
                if (plan.Shortfall < Epsilon) plan.Shortfall = 0f;
            }
            else
            {
                float surplus = plan.Generated - plan.Load;
                float charge = Math.Min(surplus, AvailableChargePower(elapsedSeconds));
                plan.StorageNetPower = charge;
                plan.Discarded = surplus - charge;
            }

            return plan;
        }

        /// <summary>剩余燃料在这个时长内最多能发多少功率（1 生物质 ＝ 60 电量）。</summary>
        private static float PowerFromFuel(IEnergyGenerator generator, float elapsedSeconds)
        {
            if (elapsedSeconds <= 0f) return generator.RatedPower;
            return Math.Max(0f, generator.FuelEnergyAvailable) * 60f / elapsedSeconds;
        }

        /// <summary>
        /// 燃料发电为储能充电，返回本时刻因此多发的功率。
        ///
        /// 第一版**不定义充放电功率上限**，所以这里用「设施剩余额定容量」兜住充电功率：
        /// 储能目标是能量、发电站能给的是功率，两者之间必须有一步换算，
        /// 用额定功率兜住它既符合「不超过额定」的既有约束，也不引入设计里没有的新数。
        /// </summary>
        private float AllocateChargingFromFuel(float elapsedSeconds)
        {
            float headroom = StorageHeadroomToTarget();
            if (headroom <= 0f) return 0f;

            float extra = 0f;
            for (int i = 0; i < _generators.Count && headroom > 0f; i++)
            {
                IEnergyGenerator generator = _generators[i];
                if (generator.Kind != GeneratorKind.Fuel || !generator.IsOn || !generator.AllowsCharging) continue;

                float spare = generator.RatedPower - generator.ActualOutputPower;
                if (spare <= 0f) continue;

                float output = Math.Min(spare, PowerFromFuel(generator, elapsedSeconds));
                if (output <= 0f) continue;

                generator.ActualOutputPower += output;
                extra += output;
                headroom -= PowerToEnergy(output, elapsedSeconds);
            }

            return extra;
        }

        /// <summary>
        /// 储能朝**目标比例**还差多少电量。
        ///
        /// 存在多个充电目标时取**最小的那个**：目标是「不许超过」的上限，
        /// 取大的会让某个发电站超过自己设定的目标。
        /// </summary>
        private float StorageHeadroomToTarget()
        {
            float targetRatio = 1f;
            bool anyCharging = false;
            for (int i = 0; i < _generators.Count; i++)
            {
                IEnergyGenerator generator = _generators[i];
                if (generator.Kind != GeneratorKind.Fuel || !generator.IsOn || !generator.AllowsCharging) continue;
                anyCharging = true;
                targetRatio = Math.Min(targetRatio, Clamp01(generator.ChargeTargetRatio));
            }

            if (!anyCharging) return 0f;

            float headroom = 0f;
            for (int i = 0; i < _storages.Count; i++)
            {
                IEnergyStorage storage = _storages[i];
                headroom += Math.Max(0f, storage.Capacity * targetRatio - storage.Charge);
            }

            return headroom;
        }

        private float AvailableDischargePower(float elapsedSeconds)
        {
            float charge = 0f;
            for (int i = 0; i < _storages.Count; i++) charge += Math.Max(0f, _storages[i].Charge);
            if (elapsedSeconds <= 0f) return charge > 0f ? float.PositiveInfinity : 0f;
            return charge * 60f / elapsedSeconds;
        }

        private float AvailableChargePower(float elapsedSeconds)
        {
            float headroom = 0f;
            for (int i = 0; i < _storages.Count; i++)
            {
                IEnergyStorage storage = _storages[i];
                headroom += Math.Max(0f, storage.Capacity - storage.Charge);
            }

            if (elapsedSeconds <= 0f) return headroom > 0f ? float.PositiveInfinity : 0f;
            return headroom * 60f / elapsedSeconds;
        }

        /// <summary>把计划落到设施上：燃料消耗、储能电量、每个对象的实际功率。</summary>
        private void Apply(float elapsedSeconds, Plan plan)
        {
            // 燃料发电按实际输出扣燃料（1 生物质 ＝ 60 电量），扣不了超过剩余量的部分。
            for (int i = 0; i < _generators.Count; i++)
            {
                IEnergyGenerator generator = _generators[i];
                if (generator.Kind != GeneratorKind.Fuel) continue;
                float consumed = Math.Min(PowerToEnergy(generator.ActualOutputPower, elapsedSeconds),
                    Math.Max(0f, generator.FuelEnergyAvailable));
                generator.FuelEnergyConsumed = consumed;
                generator.FuelEnergyAvailable -= consumed;
            }

            float totalCapacity = 0f;
            for (int i = 0; i < _storages.Count; i++) totalCapacity += _storages[i].Capacity;

            for (int i = 0; i < _storages.Count; i++)
            {
                IEnergyStorage storage = _storages[i];
                if (elapsedSeconds <= 0f || Math.Abs(plan.StorageNetPower) < Epsilon || totalCapacity <= 0f)
                {
                    storage.State = StorageState.Idle;
                    storage.ActualPower = 0f;
                    continue;
                }

                float share = plan.StorageNetPower * (storage.Capacity / totalCapacity);
                float delta = PowerToEnergy(share, elapsedSeconds);
                float next = Clamp(storage.Charge + delta, 0f, storage.Capacity);
                storage.ActualPower = EnergyToPower(next - storage.Charge, elapsedSeconds);
                storage.Charge = next;
                storage.State = delta > 0f ? StorageState.Charging
                    : delta < 0f ? StorageState.Discharging : StorageState.Idle;
            }

            // 用电对象：获准供电的按它自己请求的功率计，被停机的耗电归零。
            for (int i = 0; i < _consumers.Count; i++)
            {
                IEnergyConsumer consumer = _consumers[i];
                if (consumer.IsStoppedByShortage || !consumer.IsDemandActive)
                {
                    consumer.IsPowered = false;
                    consumer.ActualPower = 0f;
                    continue;
                }

                consumer.IsPowered = true;
                consumer.ActualPower = Math.Max(0f, consumer.RequestedPower);
            }
        }

        private EnergyGridSnapshot Build(Plan plan, bool forceShortfall)
        {
            float stored = 0f, capacity = 0f, charging = 0f, discharging = 0f;
            for (int i = 0; i < _storages.Count; i++)
            {
                IEnergyStorage storage = _storages[i];
                stored += storage.Charge;
                capacity += storage.Capacity;
                if (storage.ActualPower > 0f) charging += storage.ActualPower;
                else discharging += -storage.ActualPower;
            }

            float consumed = 0f;
            for (int i = 0; i < _consumers.Count; i++) consumed += _consumers[i].ActualPower;

            // 「本时刻是否缺电」的判据是**有没有对象被迫停机**，而不是「计划里还剩多少缺口」：
            // 停机之后缺口必然被抹平，只看 gap 会永远报「不缺电」。
            bool hasShortfall = forceShortfall || plan.Shortfall > Epsilon || _stopped.Count > 0;

            return new EnergyGridSnapshot(plan.Generated, consumed, stored, capacity, plan.Discarded,
                charging, discharging, _stopped.ToArray(), hasShortfall);
        }

        /// <summary>电量 ＝ 功率 × 秒数 ÷ 60（规格里的统一换算）。</summary>
        public static float PowerToEnergy(float power, float elapsedSeconds) => power * elapsedSeconds / 60f;

        /// <summary>功率 ＝ 电量 × 60 ÷ 秒数。</summary>
        public static float EnergyToPower(float energy, float elapsedSeconds) =>
            elapsedSeconds <= 0f ? 0f : energy * 60f / elapsedSeconds;

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;

        private static float Clamp(float value, float min, float max) =>
            value < min ? min : value > max ? max : value;
    }
}
