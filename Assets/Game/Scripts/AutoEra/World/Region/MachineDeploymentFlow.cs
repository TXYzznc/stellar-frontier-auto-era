using System;
using AutoEra.Machines;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>落位流程的结局。分别可辨是刻意的：界面要把它们翻译成不同的可展示原因，而不是一句「失败」。</summary>
    public enum MachineDeploymentOutcome
    {
        /// <summary>已写入区域并让花名册转入已部署。</summary>
        Deployed,
        /// <summary>区域本身不可用（未初始化或已释放）。</summary>
        InvalidRegion,
        /// <summary>机器不在花名册里。</summary>
        NotInRoster,
        /// <summary>机器已经在这个区域里（重复提交走到这里）。</summary>
        AlreadyBound,
        /// <summary>机器已损坏，按领域规则不能部署。</summary>
        Destroyed,
        /// <summary>机器已绑定到另一个区域。</summary>
        BoundElsewhere,
        /// <summary>位置／朝向不合法（越界或占地冲突）。</summary>
        InvalidPlacement,
    }

    /// <summary>
    /// 机器落位流程：**校验**与**领域提交**两段分离。
    ///
    /// 校验完全交给现成的 <see cref="RegionPlacementPreview"/>（吸附、旋转、越界、重叠，以及
    /// 「确认时再校验一次」的既有语义）；本类只负责三件它不该知道的事：
    /// <list type="number">
    /// <item>前置条件——机器在册、未部署、定义已配置占地；</item>
    /// <item>占地从哪来——机器定义的交互占地，而不是调用点各自写常量；</item>
    /// <item>提交去向——<see cref="InitialRegion.DeployMachine"/>。</item>
    /// </list>
    ///
    /// **不做结算**：不扣费、不判定建造条件、不写存档。那是调用方与领域各自的事。
    ///
    /// 视图生成、导航绑定与运行时创建**不在这里**：它们是部署成功之后的派生步骤，
    /// 由区域侧的表现层在拿到 <see cref="RegionObject"/> 之后接着做（见变更的 design.md D2–D5）。
    /// 本类只保证「领域部署」这一步的成败与原因都说得清楚。
    /// </summary>
    public sealed class MachineDeploymentFlow : IDisposable
    {
        private readonly AutoEraWorldSession _session;
        private readonly InitialRegion _region;
        private RegionPlacementPreview _preview;
        private PersistentId _machineId;
        private MachineDeploymentOutcome _pendingOutcome;
        private RegionObject _confirmedModel;

        /// <summary>最近一次提交的结局；从未提交过时为 null。界面用它区分「已部署」「位置不合法」「已绑定到别处」等。</summary>
        public MachineDeploymentOutcome? LastOutcome { get; private set; }

        public MachineDeploymentFlow(AutoEraWorldSession session, InitialRegion region)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _region = region ?? throw new ArgumentNullException(nameof(region));
        }

        /// <summary>是否正处于一次落位中（预览已建立）。</summary>
        public bool IsActive => _preview != null;

        /// <summary>本次落位的机器；未开始时为 <see cref="PersistentId.Invalid"/>。</summary>
        public PersistentId MachineId => _machineId;

        /// <summary>预览（界面读 <c>IsValid</c>/<c>Reason</c>/<c>Position</c>/<c>Yaw</c>）；未开始时为 null。</summary>
        public RegionPlacementPreview Preview => _preview;

        /// <summary>本次落位使用的占地（来自机器定义）。</summary>
        public Vector2 Size { get; private set; }

        /// <summary>
        /// 开始一次落位。前置不满足时返回 false 并给出**可展示的原因**，且不留下半个预览。
        /// </summary>
        public bool TryBegin(PersistentId machineId, out string reason)
        {
            reason = null;
            Cancel();
            if (_region == null || !_region.IsActive) { reason = "区域不可用"; return false; }
            if (!_session.Machines.TryGet(machineId, out MachineInstance machine)) { reason = "机器不在花名册里"; return false; }
            if (machine.Deployed) { reason = "该机器已部署，不能重复落位"; return false; }
            if (!machine.Definition.HasFootprint) { reason = "该型号未配置交互占地，无法做落位校验"; return false; }

            Size = machine.Definition.Footprint;
            _machineId = machineId;
            _pendingOutcome = MachineDeploymentOutcome.NotInRoster; // 占位；Commit 前必被覆盖
            LastOutcome = null;
            _confirmedModel = null;
            _preview = new RegionPlacementPreview(_region, Size, OnConfirmed);
            return true;
        }

        /// <summary>
        /// 提交。会先走一遍预览自己的校验（含确认时再校验），再调 <see cref="InitialRegion.DeployMachine"/>。
        /// 成功即返回 true 并给出区域的 <see cref="RegionObject"/>；失败给出可展示原因。
        /// </summary>
        public bool TryCommit(out RegionObject model, out string reason)
        {
            model = null;
            reason = null;
            if (_preview == null) { reason = "尚未开始落位"; return false; }
            if (!_preview.IsValid) { reason = _preview.Reason ?? "当前位置不可放置"; return false; }

            _pendingOutcome = MachineDeploymentOutcome.InvalidRegion;
            _confirmedModel = null;
            bool confirmed = _preview.Confirm();   // 内部会再校验一次，通过才调用 OnConfirmed
            _preview = null;
            if (!confirmed) { reason = "确认时位置已经失效（场地变化），请重新选点"; return false; }

            LastOutcome = _pendingOutcome;
            model = _confirmedModel;
            if (_pendingOutcome != MachineDeploymentOutcome.Deployed)
            {
                reason = Describe(_pendingOutcome);
                model = null;
                return false;
            }

            return true;
        }

        /// <summary>放弃本次落位。幂等。已提交过的结局（<see cref="LastOutcome"/>）不会再被清掉，它是一次历史事实。</summary>
        public void Cancel()
        {
            // 预览由本次落位拥有：取消时释放；但提交路径里预览已自行 Dispose，重复释放无害。
            if (_preview != null) { _preview.Dispose(); _preview = null; }
            _machineId = PersistentId.Invalid;
            Size = Vector2.zero;
            _confirmedModel = null;
        }

        public void Dispose() => Cancel();

        private void OnConfirmed(Vector2 position, float yaw)
        {
            RegionMachineDeploymentResult result = _region.DeployMachine(_machineId, position, Size, out RegionObject model, yaw);
            _confirmedModel = model;
            _pendingOutcome = result switch
            {
                RegionMachineDeploymentResult.Bound => MachineDeploymentOutcome.Deployed,
                RegionMachineDeploymentResult.AlreadyBound => MachineDeploymentOutcome.AlreadyBound,
                RegionMachineDeploymentResult.InvalidRegion => MachineDeploymentOutcome.InvalidRegion,
                RegionMachineDeploymentResult.InvalidMachine => MachineDeploymentOutcome.NotInRoster,
                RegionMachineDeploymentResult.Destroyed => MachineDeploymentOutcome.Destroyed,
                RegionMachineDeploymentResult.BoundElsewhere => MachineDeploymentOutcome.BoundElsewhere,
                _ => MachineDeploymentOutcome.InvalidPlacement,
            };
        }

        /// <summary>把结局翻译成玩家可读的原因。界面直接用这个字符串，不再各自拼。</summary>
        public static string Describe(MachineDeploymentOutcome outcome) => outcome switch
        {
            MachineDeploymentOutcome.Deployed => string.Empty,
            MachineDeploymentOutcome.InvalidRegion => "区域不可用",
            MachineDeploymentOutcome.NotInRoster => "机器不在花名册里（可能已被移除）",
            MachineDeploymentOutcome.AlreadyBound => "该机器已经在这个区域里",
            MachineDeploymentOutcome.Destroyed => "机器已损坏，不能部署",
            MachineDeploymentOutcome.BoundElsewhere => "该机器已绑定到其它区域",
            MachineDeploymentOutcome.InvalidPlacement => "位置不合法：超出区域边界或与已有对象占地冲突",
            _ => "落位失败",
        };
    }
}
