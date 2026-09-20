using System;
using System.Collections.Generic;
using AutoEra.Algorithms;

namespace AutoEra.UI
{
    /// <summary>算法域内发生变化的区域。</summary>
    public enum AlgorithmDomainSection
    {
        /// <summary>模板列表发生变化。</summary>
        List,

        /// <summary>当前选中模板的详情发生变化。</summary>
        Detail,
    }

    /// <summary>模板列表的一行：稳定身份 + 名称 + 版本 + 是否系统模板。</summary>
    public readonly struct UiAlgorithmTemplateRow
    {
        public UiAlgorithmTemplateRow(ulong id, string name, string version, bool isSystem)
        {
            Id = id;
            Name = name;
            Version = version;
            IsSystem = isSystem;
        }

        public ulong Id { get; }
        public string Name { get; }
        public string Version { get; }

        /// <summary>系统模板只读；玩家模板可改名/删除。</summary>
        public bool IsSystem { get; }
    }

    /// <summary>算法域的只读快照。</summary>
    public readonly struct AlgorithmDomainSnapshot
    {
        public AlgorithmDomainSnapshot(
            UiDataState state,
            string unavailableReason,
            IReadOnlyList<UiAlgorithmTemplateRow> templates,
            IReadOnlyList<UiDetailField> detail,
            int selectedIndex)
        {
            State = state;
            UnavailableReason = unavailableReason;
            Templates = templates;
            Detail = detail;
            SelectedIndex = selectedIndex;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }
        public IReadOnlyList<UiAlgorithmTemplateRow> Templates { get; }
        public IReadOnlyList<UiDetailField> Detail { get; }
        public int SelectedIndex { get; }

        public int Count => Templates == null ? 0 : Templates.Count;
        public bool HasSelection => SelectedIndex >= 0 && SelectedIndex < Count;

        public int SystemCount => CountBySystem(true);
        public int PlayerCount => CountBySystem(false);

        private int CountBySystem(bool system)
        {
            if (Templates == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < Templates.Count; i++)
            {
                if (Templates[i].IsSystem == system)
                {
                    count++;
                }
            }

            return count;
        }

        public static AlgorithmDomainSnapshot Unavailable(string reason) =>
            new AlgorithmDomainSnapshot(UiDataState.Unavailable, reason, null, null, -1);
    }

    /// <summary>
    /// 算法域读取模型。
    ///
    /// **接口已冻结，真实实现待算法域接线后补。** 目前的工厂总是返回诚实空实现，
    /// 原因是算法域的服务层虽然完整（`AlgorithmInstanceService` / `AlgorithmTemplateLibrary`
    /// 都有实现与集成测试），但**生产运行路径从未创建它们**：`MachineExecutionContext` 与
    /// 算法实例服务只在测试里被 new 出来。没有实例服务就没有可展示的实例、草稿、问题清单与
    /// 应用请求，界面能做的最诚实的事就是把这件事说明白。
    ///
    /// 接线完成后只需替换 <see cref="AlgorithmReadModels.Create"/> 的分支，
    /// 四个算法界面不需要任何改动——它们只依赖本接口。
    /// </summary>
    public interface IAlgorithmReadModel : IDisposable
    {
        AlgorithmDomainSnapshot Snapshot { get; }

        event Action<AlgorithmDomainSection> Changed;

        int SelectedIndex { get; }

        void Refresh();

        bool Select(ulong templateId);

        void ClearSelection();
    }

    /// <summary>算法域未接入时的诚实空实现：只报 Unavailable，不伪造模板与实例。</summary>
    internal sealed class UnavailableAlgorithmReadModel : IAlgorithmReadModel
    {
        public UnavailableAlgorithmReadModel(string reason)
        {
            Snapshot = AlgorithmDomainSnapshot.Unavailable(reason);
        }

        public AlgorithmDomainSnapshot Snapshot { get; }

        public event Action<AlgorithmDomainSection> Changed
        {
            add { }
            remove { }
        }

        public int SelectedIndex => -1;

        public void Refresh() { }

        public bool Select(ulong templateId) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }

    /// <summary>算法界面的统一入口：把「算法域为什么不可用」讲清楚。</summary>
    public static class AlgorithmReadModels
    {
        /// <summary>算法域已实现但未接进生产运行路径，界面据此说明原因——这是可展示的真实状态。</summary>
        public const string NotWiredReason =
            "算法域尚未接入世界运行路径：机器的执行上下文与算法实例服务在生产中还没有创建者，"
            + "因此编辑、诊断与模板实例化暂不可用。";

        public static IAlgorithmReadModel Create(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableAlgorithmReadModel("没有界面会话：算法数据不可用。");
            }

            if (!session.HasWorld)
            {
                return new UnavailableAlgorithmReadModel("算法属于某个世界里的机器，请先从主菜单进入区域。");
            }

            // 世界已就绪，但算法域的服务还没有被世界或区域创建——原因由常量给出，界面直接展示。
            return new UnavailableAlgorithmReadModel(NotWiredReason);
        }
    }
}
