using AutoEra.Algorithms;

namespace AutoEra.UI
{
    /// <summary>
    /// 「为某个算法实例的某个绑定端点挑一件组件」的请求（规格 13-算法编辑器/节点组件选择）。
    ///
    /// 它描述**目标端点**（实例 + BindingKey + 端点类别），不描述要挑哪一件——挑哪一件是玩家
    /// 在页面里做的决定。选择器直接通过共享的算法实例服务回写 <c>Rebind</c>（组件 Id），
    /// 目标对象（<c>TargetId</c>）由后续的世界对象选择器补齐，因此这里不需要结果回传对象：
    /// 调用方（绑定面板）靠共享实例服务的 <c>Changed</c> 事件刷新，而不是等回调。
    /// </summary>
    public sealed class AutoEraAlgorithmBindingPickRequest
    {
        public AutoEraAlgorithmBindingPickRequest(ulong instanceId, string bindingKey, AlgorithmNodeKind kind)
        {
            InstanceId = instanceId;
            BindingKey = bindingKey ?? string.Empty;
            Kind = kind;
        }

        public ulong InstanceId { get; }

        /// <summary>目标端点（Input/Effector 的 BindingKey）。</summary>
        public string BindingKey { get; }

        /// <summary>端点类别，用于把候选筛选成传感器（Input）或效应器（Effector）。</summary>
        public AlgorithmNodeKind Kind { get; }

        /// <summary>动机文字，用于页面标题与一句总述。</summary>
        public string Intent =>
            (Kind == AlgorithmNodeKind.Input ? "传感器" : "效应器") + " · " + BindingKey;
    }
}
