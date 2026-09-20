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
    }
}
