using System.Globalization;
using AutoEra.Machines;

namespace AutoEra.UI
{
    /// <summary>
    /// 界面显示层的最小格式化入口。
    ///
    /// 界面通用合同要求「金额、时间、数量、单位由显示模型格式化」，界面不自行拼字符串、
    /// 也不把未知值伪装成零。这里只放第一阶段真正用到的几个格式化，随接入推进扩展。
    /// </summary>
    public static class AutoEraUiFormat
    {
        /// <summary>无数据／不可用时的统一占位符（与契约里的样例文案一致）。</summary>
        public const string Missing = "—";

        public static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>完整度：领域用 0..100 的 double，界面显示为整数百分比。</summary>
        public static string Integrity(double value) =>
            value.ToString("0", CultureInfo.InvariantCulture) + "%";

        /// <summary>世界时间：从世界毫秒换算为「第 N 日 HH:MM」。日长固定 24 小时。</summary>
        public static string WorldTime(long worldMilliseconds)
        {
            long totalMinutes = worldMilliseconds / 60000L;
            long day = totalMinutes / (24L * 60L) + 1L;
            long hour = totalMinutes / 60L % 24L;
            long minute = totalMinutes % 60L;
            return string.Concat(
                "第", day.ToString(CultureInfo.InvariantCulture),
                "日 ", hour.ToString("00", CultureInfo.InvariantCulture),
                ":", minute.ToString("00", CultureInfo.InvariantCulture));
        }

        public static string RunState(MachineRunState state)
        {
            switch (state)
            {
                case MachineRunState.Running: return "运行中";
                case MachineRunState.Sleeping: return "休眠";
                default: return "已停止";
            }
        }

        /// <summary>
        /// 一段时长（世界毫秒）的人读写法。
        ///
        /// 用在能源停机记录这类「持续了多久」上：不足一分钟按秒、不足一小时按分秒、
        /// 再长按小时分钟——三档够用，也不会把 3 分 20 秒写成「200 秒」。
        /// 负数按 0 处理（时钟回退或记录顺序异常时不要把负时长显示给玩家）。
        /// </summary>
        public static string Duration(long milliseconds)
        {
            long totalSeconds = milliseconds / 1000L;
            if (totalSeconds < 0L) totalSeconds = 0L;

            if (totalSeconds < 60L)
            {
                return totalSeconds.ToString(CultureInfo.InvariantCulture) + " 秒";
            }

            if (totalSeconds < 3600L)
            {
                long minutes = totalSeconds / 60L;
                long seconds = totalSeconds % 60L;
                return string.Concat(
                    minutes.ToString(CultureInfo.InvariantCulture), " 分 ",
                    seconds.ToString("00", CultureInfo.InvariantCulture), " 秒");
            }

            long hours = totalSeconds / 3600L;
            long restMinutes = totalSeconds % 3600L / 60L;
            return string.Concat(
                hours.ToString(CultureInfo.InvariantCulture), " 小时 ",
                restMinutes.ToString(CultureInfo.InvariantCulture), " 分");
        }

        /// <summary>索引行的一句状态摘要：位置 + 运行态 + 异常提示。</summary>
        public static string MachineSummary(MachineInstance machine)
        {
            if (machine == null) return Missing;

            string placement = machine.Deployed ? "已部署" : "库中";
            string run = RunState(machine.RequestedRunState);
            if (!machine.Powered && machine.Deployed) return placement + " · 已断电";
            if (!machine.Connected && machine.Deployed) return placement + " · 无连接";
            return placement + " · " + run;
        }

        /// <summary>
        /// 槽位的人读名字（「核心槽 0」）。集中在这里是因为机器整备、硬件修改确认与组件选择器
        /// 三处都要说同一个槽位——各写一份迟早会出现「核心槽 1」与「核心 1 号槽」两种说法。
        /// </summary>
        public static string Slot(HardwareKind kind, int index) => SlotKind(kind) + "槽 " + index;

        /// <summary>硬件类别的中文名。</summary>
        public static string SlotKind(HardwareKind kind) => kind switch
        {
            HardwareKind.Sensor => "传感器",
            HardwareKind.Core => "核心",
            HardwareKind.Effector => "执行器",
            _ => "槽",
        };

        /// <summary>
        /// 按键的人读名字。鼠标键在引擎里是 <c>KeyCode.Mouse0..2</c>，直接 ToString 会显示成
        /// 「Mouse1」——玩家看到的是「鼠标右键」。
        /// </summary>
        public static string KeyLabel(UnityEngine.KeyCode key) => key switch
        {
            UnityEngine.KeyCode.Mouse0 => "鼠标左键",
            UnityEngine.KeyCode.Mouse1 => "鼠标右键",
            UnityEngine.KeyCode.Mouse2 => "鼠标中键",
            UnityEngine.KeyCode.Escape => "Esc",
            UnityEngine.KeyCode.Return => "Enter",
            UnityEngine.KeyCode.Space => "空格",
            UnityEngine.KeyCode.Tab => "Tab",
            UnityEngine.KeyCode.Backspace => "退格",
            UnityEngine.KeyCode.Delete => "Delete",
            UnityEngine.KeyCode.LeftShift => "左 Shift",
            UnityEngine.KeyCode.RightShift => "右 Shift",
            UnityEngine.KeyCode.LeftControl => "左 Ctrl",
            UnityEngine.KeyCode.RightControl => "右 Ctrl",
            UnityEngine.KeyCode.LeftAlt => "左 Alt",
            UnityEngine.KeyCode.RightAlt => "右 Alt",
            UnityEngine.KeyCode.UpArrow => "方向键上",
            UnityEngine.KeyCode.DownArrow => "方向键下",
            UnityEngine.KeyCode.LeftArrow => "方向键左",
            UnityEngine.KeyCode.RightArrow => "方向键右",
            _ => key.ToString(),
        };

        /// <summary>警报等级的显示名（规格 15：提醒／警告／严重）。</summary>
        public static string AlertSeverity(Alerts.AlertSeverity severity) => severity switch
        {
            Alerts.AlertSeverity.Critical => "严重",
            Alerts.AlertSeverity.Warning => "警告",
            _ => "提醒",
        };

        /// <summary>硬件修改被领域拒绝时的可展示原因。</summary>
        public static string ManagementResult(MachineManagementResult result) => result switch
        {
            MachineManagementResult.Completed => "已完成",
            MachineManagementResult.WaitingForSafeStop => "等待安全停机",
            MachineManagementResult.MustStop => "机器还没有停下来",
            MachineManagementResult.InvalidOrigin => "来源不允许：整备环境只接受库来源，现场只接受现场来源",
            MachineManagementResult.InvalidSlot => "槽位不存在",
            MachineManagementResult.Occupied => "槽位已经被占用",
            MachineManagementResult.AlreadyInstalled => "该组件已经装在别的槽位上",
            MachineManagementResult.MissingComponent => "槽位里没有组件",
            MachineManagementResult.CapacityInUse => "卸下会低于当前已用容量",
            MachineManagementResult.ComputeInUse => "卸下会低于已保留的算力",
            MachineManagementResult.LogicCapacityInUse => "卸下会低于已应用的逻辑算力",
            MachineManagementResult.Destroyed => "机器已损坏",
            MachineManagementResult.NotDeployed => "机器尚未部署",
            MachineManagementResult.NotActivated => "机器尚未激活",
            MachineManagementResult.Disconnected => "机器没有连接",
            MachineManagementResult.CoreHasNoSwitch => "该核心没有独立开关",
            MachineManagementResult.RepairRequired => "需要先修复",
            MachineManagementResult.InvalidState => "机器当前状态不允许修改硬件",
            _ => "修改未通过",
        };
    }
}
