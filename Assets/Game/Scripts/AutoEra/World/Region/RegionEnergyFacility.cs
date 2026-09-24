using System;
using AutoEra.Energy;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>场景对象声明自己是哪一类能源设施（规格 06-能源储存与物流）。</summary>
    public enum RegionEnergyFacilityKind
    {
        /// <summary>免费环境能源（初始太阳能发电器）：按日照给出可用出力，不消耗燃料。</summary>
        EnvironmentGenerator = 0,

        /// <summary>燃料发电站（基础生物质发电机）：只补足剩余需求，按实际输出扣燃料。</summary>
        FuelGenerator = 1,

        /// <summary>蓄电设施（基础蓄电池）：吸收盈余、补足缺口。</summary>
        Battery = 2,
    }

    /// <summary>
    /// 把场景里的一个对象声明成**能源设施**：它带着设计给的额定功率／燃料／容量，
    /// 参与所在区域电网的结算。
    ///
    /// 为什么参数放在场景组件上而不是代码里：它们是**内容**（初始基地有两台发电设施，
    /// 它们的功率与初始燃料是设计数值），而代码只负责结算规则。
    /// 功率数值与 <see cref="FirstVersionEnergy"/> 同源——那边给出基准值，
    /// 场景可以覆盖，但默认值就是设计值，不会出现「文档说 15、代码说 20」。
    ///
    /// **燃料是运行期状态**：`FuelGenerator` 保存已投入但尚未消耗完的电量，
    /// 启停不损失剩余燃料。它现在只活在实例里（存档接入时再落盘），
    /// 所以这里不假装它已经被持久化。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RegionEnergyFacility : MonoBehaviour
    {
        [SerializeField] private RegionEnergyFacilityKind _kind = RegionEnergyFacilityKind.EnvironmentGenerator;

        [Tooltip("发电设施的额定功率。环境能源按日照给出可用出力。")]
        [SerializeField] private float _ratedPower = FirstVersionEnergy.SolarDaylightPower;

        [Tooltip("燃料发电设施的初始燃料（生物质数量，1 生物质 ＝ 60 电量）。")]
        [SerializeField] private float _initialBiomass = FirstVersionEnergy.StartupBiomass;

        [Tooltip("蓄电设施的电量上限。")]
        [SerializeField] private float _capacity = FirstVersionEnergy.BatteryCapacity;

        [Tooltip("蓄电设施的初始电量。")]
        [SerializeField] private float _initialCharge;

        [Tooltip("该发电设施是否允许主动为储能充电（第一版默认不允许）。")]
        [SerializeField] private bool _allowsCharging;

        [Range(0f, 1f)]
        [Tooltip("燃料充电的目标储能比例；只有开启充电许可时才有意义。")]
        [SerializeField] private float _chargeTargetRatio;

        private EnvironmentGenerator _environment;
        private FuelGenerator _fuel;
        private BatteryStorage _battery;

        public RegionEnergyFacilityKind Kind => _kind;

        /// <summary>本设施是否为发电设施。</summary>
        public bool IsGenerator => _kind != RegionEnergyFacilityKind.Battery;

        /// <summary>
        /// 本设施是否开放**充电许可与目标储电比例**（规格：仅燃料设施开放）。
        /// 环境能源没有燃料充电这回事，储能设施自己就是被充的那一方。
        /// </summary>
        public bool SupportsChargingPolicy => _kind == RegionEnergyFacilityKind.FuelGenerator;

        /// <summary>场景对象的名字，供界面显示（拿不到时返回空串，不编造）。</summary>
        public string DisplayName => gameObject != null ? gameObject.name : string.Empty;

        /// <summary>本设施产生的发电对象；储能设施返回 null。</summary>
        public IEnergyGenerator Generator =>
            _environment != null ? (IEnergyGenerator)_environment : (IEnergyGenerator)_fuel;

        /// <summary>本设施产生的储能对象；发电设施返回 null。</summary>
        public IEnergyStorage Storage => _battery;

        /// <summary>是否已经绑定过区域对象身份。</summary>
        public bool IsInitialized => _environment != null || _fuel != null || _battery != null;

        /// <summary>绑定的区域对象身份；未初始化时为 Invalid。</summary>
        public PersistentId ObjectId { get; private set; } = PersistentId.Invalid;

        /// <summary>
        /// 用**区域对象身份**初始化本设施。重复调用是幂等的：
        /// 实体可能在一次区域生命周期里被重新显示，而设施状态（尤其燃料）不该被重置。
        /// </summary>
        public void Initialize(PersistentId objectId)
        {
            if (!objectId.IsValid) throw new ArgumentException("A valid region object id is required.", nameof(objectId));
            if (IsInitialized) return;

            switch (_kind)
            {
                case RegionEnergyFacilityKind.EnvironmentGenerator:
                    if (_ratedPower < 0f || float.IsNaN(_ratedPower) || float.IsInfinity(_ratedPower))
                        throw new InvalidOperationException("Invalid environment generator power: " + _ratedPower);
                    _environment = new EnvironmentGenerator(objectId, _ratedPower) { EnvironmentPower = 0f };
                    break;
                case RegionEnergyFacilityKind.FuelGenerator:
                    if (_ratedPower < 0f || float.IsNaN(_ratedPower) || float.IsInfinity(_ratedPower))
                        throw new InvalidOperationException("Invalid fuel generator power: " + _ratedPower);
                    if (_initialBiomass < 0f)
                        throw new InvalidOperationException("Invalid fuel amount: " + _initialBiomass);
                    _fuel = new FuelGenerator(objectId, _ratedPower, FirstVersionEnergy.BiomassToEnergy(_initialBiomass))
                    {
                        AllowsCharging = _allowsCharging,
                        ChargeTargetRatio = _chargeTargetRatio,
                    };
                    break;
                case RegionEnergyFacilityKind.Battery:
                    if (_capacity <= 0f || float.IsNaN(_capacity) || float.IsInfinity(_capacity))
                        throw new InvalidOperationException("Invalid battery capacity: " + _capacity);
                    _battery = new BatteryStorage(objectId, _capacity, _initialCharge);
                    break;
                default:
                    throw new InvalidOperationException("Unknown energy facility kind: " + _kind);
            }

            ObjectId = objectId;
        }

        /// <summary>按当前世界时间更新环境能源的可用出力（只对太阳能这类设施有意义）。</summary>
        public bool UpdateEnvironment(long worldMilliseconds)
        {
            if (_environment == null) return false;
            return _environment.UpdateEnvironment(worldMilliseconds);
        }

        /// <summary>剩余燃料折算成的生物质数量（燃料发电设施；其余为 0）。</summary>
        public float RemainingBiomass => _fuel != null ? _fuel.RemainingBiomass : 0f;

        /// <summary>当前电量（蓄电设施；其余为 0）。</summary>
        public float Charge => _battery != null ? _battery.Charge : 0f;

        private void OnDestroy()
        {
            _environment = null;
            _fuel = null;
            _battery = null;
            ObjectId = PersistentId.Invalid;
        }
    }
}
