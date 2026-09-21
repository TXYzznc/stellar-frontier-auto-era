using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法工作台（规格 13-算法编辑器：编辑与诊断两种模式、公开参数为精简内容页）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 数据来源是**这台机器的区域运行时**：`RegionMachineRuntimeRegistry` 在部署时创建的
    /// `MachineExecutionContext` 与 `AlgorithmInstanceService`。机器身份取区域当前的选中对象
    /// （稳定 Id，不按名字查找）。因此本页现在有三种真实状态，而不是只有「整页不可用」：
    /// <list type="bullet">
    /// <item>Unavailable：缺会话／世界／区域／运行时／选中的机器——各自写明缺什么；</item>
    /// <item>Empty：运行时在，但这台机器还没有算法实例（实例由模板实例化创建，模板库还没接线）；</item>
    /// <item>Ready：有真实实例，节点栏列出实例与版本，检视器列出机器、算力占用与导航状态。</item>
    /// </list>
    /// 编辑与诊断的**写入口**（改图、应用、诊断运行）仍未接线，因此业务按钮依旧由
    /// <c>DisableDomainActions</c> 处置——但只读部分是真实数据。
    ///
    /// 注：`Content_AlgorithmGraph` 上挂的是 <see cref="AutoEraGraphLayoutGroup"/>——规格提到
    /// `GraphLayoutGroup` 是设计提出、仓库未实现的组件，原型阶段用它满足「Content_ 必须有
    /// LayoutGroup」的结构契约，同时不覆写画布自由坐标。
    /// </summary>
    public sealed partial class AlgorithmEditorForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 算法编辑、1 公开参数。</summary>
        public const int PageEditor = 0;
        public const int PagePublicParameters = 1;

        private static readonly int[] NavigationPageIndex = { PageEditor, -1 };

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private IAlgorithmReadModel _algorithms;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_navButtons != null)
            {
                for (int i = 0; i < _navButtons.Length; i++)
                {
                    Button button = _navButtons[i];
                    if (button == null || i >= NavigationPageIndex.Length)
                    {
                        continue;
                    }

                    int page = NavigationPageIndex[i];
                    if (page < 0)
                    {
                        continue;
                    }

                    button.onClick.AddListener(() => ShowEditorPage(page));
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageEditor;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null ? _navButtons[0].gameObject : null);

            _algorithms = AlgorithmReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _algorithms.Changed += OnAlgorithmSectionChanged;

            Render(_algorithms.Snapshot);
            DisableDomainActions();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseAlgorithms();

        protected override void OnAutoEraRecycle()
        {
            ReleaseAlgorithms();
            base.OnAutoEraRecycle();
        }

        private void ReleaseAlgorithms()
        {
            if (_algorithms == null)
            {
                return;
            }

            _algorithms.Changed -= OnAlgorithmSectionChanged;
            _algorithms.Dispose();
            _algorithms = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowEditorPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>算法域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlgorithmDataState => _algorithms?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnAlgorithmSectionChanged(AlgorithmDomainSection section) => Render(_algorithms.Snapshot);

        /// <summary>点一行实例 → 交给读模型按稳定 Id 选中；它不认识行号，只认识身份。</summary>
        private void OnInstanceRowClicked(int index)
        {
            AlgorithmDomainSnapshot snapshot = _algorithms.Snapshot;
            if (snapshot.Instances == null || index < 0 || index >= snapshot.Instances.Count)
            {
                return;
            }

            _algorithms.Select(snapshot.Instances[index].Id);
        }

        /// <summary>
        /// 三种状态各自渲染，不再一律切 Disabled：
        /// Unavailable＝缺能力（写清缺什么）；Empty＝运行时在、实例还没有（写清为什么没有）；
        /// Ready＝有真实实例，节点栏列实例、检视器列机器与版本、问题栏陈述诊断尚未运行。
        /// 把这三者混成一个「整页不可用」正是接线前的老行为。
        /// </summary>
        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            if (snapshot.State == UiDataState.Unavailable)
            {
                string reason = snapshot.UnavailableReason ?? "算法实例暂不可用。";

                ShowPageUnavailable(reason,
                    _algorithmEditorLoadingState, _algorithmEditorEmptyState, _algorithmEditorErrorState,
                    _algorithmEditorSuccessState, _algorithmEditorDisabledState,
                    _algorithmEditorNodesBody, _algorithmEditorInspectorBody, _algorithmEditorProblemsBody);

                ShowPageUnavailable(reason,
                    _publicParametersLoadingState, _publicParametersEmptyState, _publicParametersErrorState,
                    _publicParametersSuccessState, _publicParametersDisabledState,
                    _publicParametersParametersBody, _publicParametersImpactBody);

                RenderDetailRows(_algorithmEditorNodesTemplate, _algorithmEditorNodesContent, NoFields);
                RenderDetailRows(_algorithmEditorInspectorTemplate, _algorithmEditorInspectorContent, NoFields);
                RenderDetailRows(_algorithmEditorProblemsTemplate, _algorithmEditorProblemsContent, NoFields);
                RenderDetailRows(_publicParametersParametersTemplate, _publicParametersParametersContent, NoFields);
                RenderDetailRows(_publicParametersImpactTemplate, _publicParametersImpactContent, NoFields);
                return;
            }

            bool hasInstances = snapshot.InstanceCount > 0;

            SetState(_algorithmEditorLoadingState, false);
            SetState(_algorithmEditorEmptyState, !hasInstances);
            SetState(_algorithmEditorErrorState, false);
            SetState(_algorithmEditorSuccessState, hasInstances);
            SetState(_algorithmEditorDisabledState, false);

            // 节点栏：算法域真正拥有的东西就是实例列表。
            RenderListRows(_algorithmEditorNodesTemplate, _algorithmEditorNodesContent, snapshot.InstanceCount,
                (index, item) => item.Bind(index, snapshot.Instances[index].Label, snapshot.Instances[index].Status,
                    OnInstanceRowClicked));
            SetText(_algorithmEditorNodesBody, hasInstances
                ? "本机算法实例 " + snapshot.InstanceCount + " 个（选中一行查看它的版本与算力）。"
                : AlgorithmReadModels.NoInstanceReason);

            // 检视器：机器与运行时的真实状态（含算力占用与导航降级原因）。
            RenderDetailRows(_algorithmEditorInspectorTemplate, _algorithmEditorInspectorContent, snapshot.Detail);
            SetText(_algorithmEditorInspectorBody, snapshot.MachineName != null
                ? "机器：" + snapshot.MachineName
                : "机器：—");

            // 问题栏：没有实例就没有可校验的图；有实例时诊断尚未运行，不得伪装成「无问题」。
            RenderDetailRows(_algorithmEditorProblemsTemplate, _algorithmEditorProblemsContent, NoFields);
            SetText(_algorithmEditorProblemsBody, hasInstances
                ? "诊断尚未运行：本页还没有接上校验入口，因此不显示「无问题」。"
                : AlgorithmReadModels.NoInstanceReason);

            // 公开参数页与编辑页同源：参数是实例草稿的一部分。
            SetState(_publicParametersLoadingState, false);
            SetState(_publicParametersEmptyState, !hasInstances);
            SetState(_publicParametersErrorState, false);
            SetState(_publicParametersSuccessState, hasInstances);
            SetState(_publicParametersDisabledState, false);
            RenderDetailRows(_publicParametersParametersTemplate, _publicParametersParametersContent, snapshot.Detail);
            RenderDetailRows(_publicParametersImpactTemplate, _publicParametersImpactContent, NoFields);
            SetText(_publicParametersParametersBody, hasInstances
                ? "以下是从实例草稿读出的真实版本与算力占用；参数编辑入口尚未接线。"
                : AlgorithmReadModels.NoInstanceReason);
            SetText(_publicParametersImpactBody, hasInstances
                ? "影响评估尚未运行：本页还没有接上校验入口。"
                : AlgorithmReadModels.NoInstanceReason);
        }

        private static void SetText(TMPro.TMP_Text text, string value)
        {
            if (text != null)
            {
                text.SetText(value ?? string.Empty);
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
