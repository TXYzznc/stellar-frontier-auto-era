using System;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.World.Region
{
    /// <summary>
    /// 一台已部署机器的运行时：执行上下文（任务队列、算力池、传感器集）、
    /// 可空的导航、算法适配器，以及算法实例服务。
    ///
    /// **它是派生的，不是领域事实**：机器是否已部署由花名册与区域决定，
    /// 运行时只是那份事实的运作形态。因此运行时不进存档，区域就绪时按领域状态重建
    /// （见变更 design.md 的 D4）。
    ///
    /// 生命周期由 <see cref="RegionMachineRuntimeRegistry"/> 拥有。导航的推进**不在这里**：
    /// `InitialRegionScene.Advance` 已经在驱动 `RegionNavigation`，再加一处就会双重推进。
    /// </summary>
    public sealed partial class RegionMachineRuntime : IDisposable
    {
        private readonly MachineInstance _machine;
        private readonly RegionMachineNavigationBinding _binding;
        private readonly MachineHardwareRevision _hardware;
        private bool _disposed;
        private bool _advancing;
        private bool _persistentRestoreIncomplete;
        private long _worldMilliseconds;
        private readonly System.Collections.Generic.List<ulong> _pendingRemoval = new System.Collections.Generic.List<ulong>();

        internal RegionMachineRuntime(
            MachineInstance machine,
            MachineExecutionContext context,
            RegionMachineNavigationBinding binding,
            MachineNavigation navigation,
            AlgorithmMachineAdapter adapter,
            AlgorithmInstanceService instances,
            MachineHardwareRevision hardware)
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _binding = binding;
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Navigation = navigation;
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            Instances = instances;
            _hardware = hardware;
        }

        /// <summary>机器身份。</summary>
        public PersistentId MachineId => _machine.Id;

        /// <summary>任务队列、算力池与传感器集。</summary>
        public MachineExecutionContext Context { get; }

        /// <summary>导航；不可移动或尚未绑定成功时为 null（这是正常形态，见 <see cref="AlgorithmMachineAdapter.HasNavigation"/>）。</summary>
        public MachineNavigation Navigation { get; }

        /// <summary>算法意图到机器权威的适配器。</summary>
        public AlgorithmMachineAdapter Adapter { get; }

        /// <summary>本机的算法实例服务（草稿、应用请求、模板）。</summary>
        public AlgorithmInstanceService Instances { get; }
        public RegionHardwareRuntime Hardware { get; internal set; }

        /// <summary>本机是否具备导航能力。</summary>
        public bool HasNavigation => Navigation != null;

        /// <summary>
        /// 导航绑定失败的可展示原因；绑定成功或本就不需要导航时为 null。
        /// **不可移动不算失败**——那种情况下这里是 null，是否有导航能力看 <see cref="HasNavigation"/>。
        /// </summary>
        public string NavigationUnavailableReason { get; internal set; }

        /// <summary>是否处于「本可移动但没能绑上导航」的降级状态。</summary>
        public bool IsNavigationDegraded => !string.IsNullOrEmpty(NavigationUnavailableReason);

        public bool TryActivateDraft(ulong instanceId, out string reason)
        {
            reason = null;
            if (_disposed || Instances == null || !Instances.TryReadDraft(instanceId, out var draft))
            { reason = "InstanceUnavailable"; return false; }
            if (Instances.HasRuntime(instanceId)) { reason = "AlreadyActivated"; return false; }
            ulong hardware = _hardware.Read();
            if (!Adapter.ValidateBindings(draft)) { reason = "HardwareOrBindingChanged"; return false; }
            if (!AlgorithmValidator.TryCompile(draft, Instances.AvailableLogicCapacity, out var plan, out var issues))
            { reason = issues.Count > 0 ? issues[0].Code : "CompilationRejected"; return false; }
            var candidate = new AlgorithmRuntime(new PersistentId(instanceId), plan, Context.Compute,
                Adapter.CreateInstanceSink(instanceId));
            bool committed = false;
            try
            {
                committed = Instances.CommitActivation(instanceId, draft.Revision, hardware, candidate,
                    () => Adapter.Attach(candidate), _worldMilliseconds, out reason);
                return committed;
            }
            finally
            {
                if (!committed && !Instances.OwnsRuntime(instanceId, candidate))
                { Adapter.DetachInstance(instanceId); candidate.Dispose(); }
            }
        }

        /// <summary>恢复已有实例的状态；不走首次激活，因此不会补发Startup。</summary>
        public bool RestoreInstance(ulong instanceId, AlgorithmInstanceCheckpoint checkpoint, long now)
            => !_disposed && !_advancing && Instances.Restore(instanceId, checkpoint, now);

        public bool TryApplyDraft(ulong instanceId, ulong draftRevision, ulong appliedRevision, out AlgorithmApplyRequest request)
        {
            request = null;
            return !_disposed && Instances.Apply(instanceId, draftRevision, appliedRevision, out request);
        }

        public bool RemoveInstance(ulong instanceId)
        {
            if (_disposed) return false;
            if (_advancing)
            {
                if (!Instances.TryReadDraft(instanceId, out _)) return false;
                if (!_pendingRemoval.Contains(instanceId)) _pendingRemoval.Add(instanceId);
                return true;
            }
            return Instances.Remove(instanceId, () => Adapter.DetachInstance(instanceId));
        }

        internal void AdvanceWorldStep(long worldMilliseconds, double navigationSeconds)
        {
            if (_disposed || _advancing) return;
            _worldMilliseconds = worldMilliseconds;
            _advancing = true;
            try
            {
                Adapter.BeginWorldStep(worldMilliseconds);
                Hardware?.Advance(worldMilliseconds);
                if (_disposed) return;
                Instances?.Pump(worldMilliseconds);
                Adapter.Pump(worldMilliseconds, navigationSeconds);
            }
            finally
            {
                _advancing = false;
                for (int i = 0; i < _pendingRemoval.Count; i++) RemoveInstance(_pendingRemoval[i]);
                _pendingRemoval.Clear();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if(_persistentRestoreIncomplete) Context.PrepareFailedRestoreDisposal();
            _pendingRemoval.Clear();
            // 顺序有讲究：先停算法与适配器（它们订阅了导航与机器事件），再放导航绑定，最后拆执行上下文。
            // 反过来会在拆除期收到仍在派发的事件。
            Instances?.Dispose();
            Hardware?.Dispose();
            Adapter.Dispose();
            _binding?.Dispose();
            if(_persistentRestoreIncomplete) Context.DiscardFailedRestoreReservations();
            Context.Dispose();
        }
    }

    /// <summary>只跟踪插槽身份及开关，算力占用、供电和名称变化不冒充硬件修订。</summary>
    internal sealed class MachineHardwareRevision
    {
        private readonly MachineInstance _machine;
        private readonly PersistentId[][] _ids = new PersistentId[3][];
        private readonly bool[][] _enabled = new bool[3][];
        private ulong _revision;
        internal MachineHardwareRevision(MachineInstance machine)
        {
            _machine = machine;
            for (int kind = 0; kind < 3; kind++)
            {
                int count = machine.Definition.SlotCount((HardwareKind)kind);
                _ids[kind] = new PersistentId[count]; _enabled[kind] = new bool[count];
            }
        }
        internal ulong Read()
        {
            bool changed = false;
            for (int kind = 0; kind < 3; kind++)
                for (int slot = 0; slot < _ids[kind].Length; slot++)
                {
                    var component = _machine.GetComponent((HardwareKind)kind, slot);
                    var id = component?.Id ?? PersistentId.Invalid;
                    bool enabled = component?.Enabled ?? false;
                    if (_ids[kind][slot] == id && _enabled[kind][slot] == enabled) continue;
                    _ids[kind][slot] = id; _enabled[kind][slot] = enabled; changed = true;
                }
            if (changed) _revision = checked(_revision + 1);
            return _revision;
        }
        internal bool RestorePersistentRevision(ulong revision)
        {
            Read();
            if(revision<_revision) return false;
            _revision=revision;return true;
        }
    }
}
