using System.Collections.Generic;
using AutoEra.Algorithms;

namespace AutoEra.UI
{
    public readonly struct UiAlgorithmTargetCandidate
    {
        public ulong Id { get; }
        public string Name { get; }
        public string Status { get; }
        public UiAlgorithmTargetCandidate(ulong id, string name, string status) { Id = id; Name = name; Status = status; }
    }
    /// <summary>领域码只在这里转为玩家可读说明；不改变校验规则。</summary>
    public static class AlgorithmUiMessages
    {
        public static string Reason(string code)
        {
            switch (code)
            {
                case null: case "": return null;
                case "RequiredBinding": case "HardwareOrBindingChanged": return "组件或目标绑定无效，请重新选择。";
                case "InputRequired": case "MissingInput": return "必需的输入端口尚未连接。";
                case "InputUnavailable": return "当前输入没有可用数据，等待有效采样。";
                case "TypeUnitCapabilityMismatch": return "端口类型或单位不兼容。";
                case "InputOccupied": return "这个输入端口已有连线。";
                case "DataCycle": case "EventCycle": return "连线形成了不允许的循环。";
                case "LogicCapacityExceeded": case "LogicCapacity": return "机器的逻辑容量不足。";
                case "AtomicComputeExceedsCapacity": return "本次运算所需算力超过机器容量。";
                case "WaitingCompute": case "ComputeQueueFull": return "等待可用算力。";
                case "DivisionByZero": return "除数不能为零。";
                case "DefaultInvalid": return "节点默认值无效。";
                case "StaleRevision": return "草稿或硬件已变化，请检查后重试。";
                case "AlreadyActivated": return "这个实例已经生效。";
                case "InstanceUnavailable": return "算法实例已不存在。";
                case "BindingTemporarilyUnavailable": return "绑定暂时不可用。";
                case "WarningConfirmationRequired": return "请确认警告后应用。";
                case "SafePointLost": return "安全点已变化，保留原运行配置。";
                case "StateKeyRequired": return "请设置变量或状态名称。";
                case "StateTypeMismatch": return "同名状态的类型不一致。";
                default: return "配置或运行检查未通过（" + code + "）。";
            }
        }
        public static string ApplyState(AlgorithmApplyState state, string reason)
        {
            string text;
            switch (state)
            {
                case AlgorithmApplyState.None: return "无待处理应用";
                case AlgorithmApplyState.WaitingSafePoint: text = "等待安全点"; break;
                case AlgorithmApplyState.Applying: text = "正在应用"; break;
                case AlgorithmApplyState.Succeeded: text = "应用成功"; break;
                case AlgorithmApplyState.Rejected: text = "应用未通过，原配置继续运行"; break;
                case AlgorithmApplyState.Cancelled: text = "已取消应用"; break;
                default: text = "请确认警告后应用"; break;
            }
            return string.IsNullOrEmpty(reason) ? text : text + "：" + Reason(reason);
        }
    }

    public static class AlgorithmDiagnosticPresentation
    {
        public static void AppendDetail(List<UiDetailField> fields, AlgorithmDomainSnapshot snapshot)
        {
            if (!string.IsNullOrEmpty(snapshot.CommandUnavailableReason)) fields.Add(new UiDetailField("操作提示", snapshot.CommandUnavailableReason));
            if (!snapshot.LatestRun.HasValue) return;
            var run = snapshot.LatestRun.Value;
            fields.Add(new UiDetailField("选中运行", "#" + run.RunId + " · r" + run.Revision));
            fields.Add(new UiDetailField("记录时间", run.WorldMilliseconds + " 毫秒"));
            fields.Add(new UiDetailField("任务", run.TaskId == 0 ? "无独立任务" : "#" + run.TaskId + " · " + AlgorithmGraphPortView.PortLabel(run.ResultPort)));
            if (run.SourceId != 0) fields.Add(new UiDetailField("输入来源", "组件 #" + run.SourceId + " → 目标 #" + run.SourceTargetId));
            if (!snapshot.DiagnosticView && snapshot.GraphRevision != run.Revision)
                fields.Add(new UiDetailField("历史修订", "该记录属于r" + run.Revision + "；切换诊断可查看当时的图，当前草稿不标记为已执行。"));
        }
    }
}
