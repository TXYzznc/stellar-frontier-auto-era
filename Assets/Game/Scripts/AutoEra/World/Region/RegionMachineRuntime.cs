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
    public sealed class RegionMachineRuntime : IDisposable
    {
        private readonly MachineInstance _machine;
        private readonly RegionMachineNavigationBinding _binding;
        private bool _disposed;

        internal RegionMachineRuntime(
            MachineInstance machine,
            MachineExecutionContext context,
            RegionMachineNavigationBinding binding,
            MachineNavigation navigation,
            AlgorithmMachineAdapter adapter,
            AlgorithmInstanceService instances)
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _binding = binding;
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Navigation = navigation;
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            Instances = instances;
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

        /// <summary>本机是否具备导航能力。</summary>
        public bool HasNavigation => Navigation != null;

        /// <summary>
        /// 导航绑定失败的可展示原因；绑定成功或本就不需要导航时为 null。
        /// **不可移动不算失败**——那种情况下这里是 null，是否有导航能力看 <see cref="HasNavigation"/>。
        /// </summary>
        public string NavigationUnavailableReason { get; internal set; }

        /// <summary>是否处于「本可移动但没能绑上导航」的降级状态。</summary>
        public bool IsNavigationDegraded => !string.IsNullOrEmpty(NavigationUnavailableReason);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            // 顺序有讲究：先停算法与适配器（它们订阅了导航与机器事件），再放导航绑定，最后拆执行上下文。
            // 反过来会在拆除期收到仍在派发的事件。
            Instances?.Dispose();
            Adapter.Dispose();
            _binding?.Dispose();
            Context.Dispose();
        }
    }
}
