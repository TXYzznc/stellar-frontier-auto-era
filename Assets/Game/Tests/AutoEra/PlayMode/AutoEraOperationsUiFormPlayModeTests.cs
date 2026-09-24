using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 界面在真实 GF 运行时下的打开／关闭回归。
    ///
    /// 断言只覆盖**结构与生命周期**：能从 UITable 打开、根节点的 Canvas 与缩放契约、
    /// 默认焦点落在安全返回、取消后把焦点还给触发者。页面内部的具体节点与状态通道
    /// 不再在这里断言——那是门 1 契约驱动检查
    /// （AutoEraContractPrefabGate1EditModeTests）的职责，避免同一结构被两处各写一份。
    /// </summary>
    public sealed class AutoEraOperationsUiFormPlayModeTests
    {
        [UnityTest]
        public IEnumerator OperationsForms_OpenAndCloseThroughUiExtension()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            int fieldHudId = GF.UI.OpenUIForm(UIViews.FieldHudForm);
            Assert.GreaterOrEqual(fieldHudId, 0, "FieldHudForm should be opened from the generated UITable entry.");
            yield return WaitForForm(fieldHudId, expectedLoaded: true);

            GameObject openingControl = new GameObject("OperationsUiFormOpenTrigger");
            Assert.IsNotNull(EventSystem.current, "Launch must provide an EventSystem for UI input routing.");
            EventSystem.current.SetSelectedGameObject(openingControl);

            int commandHubId = GF.UI.OpenUIForm(UIViews.BaseCommandHubForm);
            Assert.GreaterOrEqual(commandHubId, 0, "BaseCommandHubForm should be opened from the generated UITable entry.");
            yield return WaitForForm(commandHubId, expectedLoaded: true);
            Assert.IsNotNull(EventSystem.current.currentSelectedGameObject, "Hub opening should assign its default focus.");

            // 00-通用合同「数据与交互」：首焦点优先安全取消／返回，其次才是本页第一个可用交互。
            Assert.AreEqual("Btn_FormBack", EventSystem.current.currentSelectedGameObject.name,
                "Hub 的默认焦点应落在安全返回按钮上。");

            var hub = GF.UI.GetUIForm(commandHubId).Logic as AutoEra.UI.BaseCommandHubForm;
            Assert.IsNotNull(hub, "The formal command hub entry must load its GF UIForm bridge.");

            // 不带会话打开中枢 → 能源页必须走「不可用」通道，并且**原因要写进那张卡片自己**。
            // 五个状态组是覆盖在内容区上的不透明卡片（520×120、alpha=1），而预制体里卡片文案是
            // 规格说明列的占位（「Disabled：—」）；只激活组、不写文案，玩家看到的就是一句占位话，
            // 写进正文的那份原因正好被卡片挡住。
            Assert.IsNotNull(hub.HubEnergyDisabledState, "Grp_HubEnergyDisabledState 必须绑进契约。");
            Assert.IsTrue(hub.HubEnergyDisabledState.activeSelf,
                "没有界面会话时，能源页必须显示为不可用，而不是伪装成「没有内容」。");
            string energyCard = StateCardText(hub.HubEnergyDisabledState);
            Assert.IsNotEmpty(energyCard, "不可用卡片必须写出真实原因，而不是留空。");
            Assert.IsFalse(energyCard.StartsWith("Disabled"),
                $"卡片停在占位文案上了：{energyCard}");
            Assert.IsFalse(hub.HubEnergySuccessState.activeSelf,
                "读一次快照不是提交，不得点亮成功卡片。");

            // The form root deliberately carries no CanvasScaler: the form is instantiated
            // under the scene's root Canvas, and a CanvasScaler without a Canvas on the same
            // GameObject is inert. Scaling is owned by the root Canvas (GF-UI-Standards/03+08).
            // 注意：GF 会在实例化 UIForm 时给实例根补一个 Canvas（运行期行为），所以这里只断言
            // CanvasScaler；「预制体根不带 Canvas」是对**资产**的要求，由门 1 静态检查判定。
            Assert.IsNull(hub.gameObject.GetComponent<CanvasScaler>(), "Form 根节点不得自带 CanvasScaler。");
            Assert.IsNotNull(GFBuiltin.RootCanvas, "场景根 Canvas 必须存在。");
            CanvasScaler rootScaler = GFBuiltin.RootCanvas.GetComponent<CanvasScaler>();
            Assert.IsNotNull(rootScaler, "场景根 Canvas 必须提供 CanvasScaler。");
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, rootScaler.uiScaleMode);
            Assert.AreEqual(new Vector2(1920f, 1080f), rootScaler.referenceResolution);

            Assert.IsTrue(hub.TryHandleIntent(AutoEra.UI.AutoEraUiIntent.Cancel),
                "Hub 必须消费取消意图。");
            yield return WaitForForm(commandHubId, expectedLoaded: false);
            Assert.AreEqual(openingControl, EventSystem.current.currentSelectedGameObject,
                "Cancel should restore the control that opened the Hub.");

            GF.UI.CloseUIForm(fieldHudId);
            yield return WaitForForm(fieldHudId, expectedLoaded: false);
            Object.Destroy(openingControl);
        }

        /// <summary>
        /// 读状态卡片上的文案。卡片是按结构定位的（状态组里只有一个说明面板、一个文本），
        /// 所以这里也按结构读——「卡片有没有写出真实原因」这件事本身就该与绑定无关。
        /// </summary>
        private static string StateCardText(GameObject state)
        {
            if (state == null)
            {
                return null;
            }

            TMPro.TMP_Text[] texts = state.GetComponentsInChildren<TMPro.TMP_Text>(true);
            return texts.Length == 0 ? null : texts[0].text;
        }

        private static IEnumerator EnsureLaunchSceneLoaded()
        {
            const string launchSceneName = "Launch";
            if (SceneManager.GetActiveScene().name == launchSceneName)
            {
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(launchSceneName, LoadSceneMode.Single);
            Assert.IsNotNull(loadOperation, "Launch must be enabled in Build Settings for the UIForm PlayMode regression.");
            yield return loadOperation;
            Assert.AreEqual(launchSceneName, SceneManager.GetActiveScene().name, "The UIForm regression must execute against the production Launch scene.");
            yield return null;
        }

        private static IEnumerator WaitForRuntimeReady()
        {
            const int maxFrames = 600;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI != null && GF.DataTable != null &&
                    GF.DataTable.HasDataTable<UITable>() &&
                    GF.DataTable.HasDataTable<UIGroupTable>() &&
                    GF.UI.HasUIGroup("Default"))
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("GF UI runtime or required UI data tables did not become ready within 600 frames.");
        }

        private static IEnumerator WaitForForm(int serialId, bool expectedLoaded)
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI.HasUIForm(serialId) == expectedLoaded)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"UI form serial {serialId} did not reach loaded={expectedLoaded} within 300 frames.");
        }
    }
}
