using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace AutoEra.Tests.PlayMode
{
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
            Assert.AreEqual("Txt_TabOverview", EventSystem.current.currentSelectedGameObject.name, "Hub default focus should be the overview tab.");

            var hub = GF.UI.GetUIForm(commandHubId).Logic as AutoEra.UI.BaseCommandHubForm;
            Assert.IsNotNull(hub, "The formal command hub entry must load its GF UIForm bridge.");
            CanvasScaler canvasScaler = hub.gameObject.GetComponent<CanvasScaler>();
            Assert.IsNotNull(canvasScaler, "AutoEra UI forms must use one standard CanvasScaler.");
            Assert.AreEqual(CanvasScaler.ScaleMode.ScaleWithScreenSize, canvasScaler.uiScaleMode);
            Assert.AreEqual(new Vector2(1920f, 1080f), canvasScaler.referenceResolution);
            long operationRequest = hub.BeginOperationRequest();
            var inProgress = new AutoEra.UI.Contracts.AutoEraUiOperationSnapshot(
                "playmode-operation", AutoEra.UI.Contracts.AutoEraUiOperationStatus.InProgress, "正在执行", 0.5f, true, false, "rule-details", "rules");
            Assert.IsTrue(hub.TryApplyOperationSnapshot(hub.CurrentFormVersion, operationRequest, inProgress, true));
            yield return null;
            Assert.IsTrue(FindRequired(hub.transform, "Btn_RuleOperationCancel").gameObject.activeSelf);
            Assert.IsTrue(FindRequired(hub.transform, "Txt_RuleOperationLongWait").gameObject.activeSelf);
            Assert.That(FindRequired(hub.transform, "Bar_RuleOperationProgress").GetComponent<Image>().fillAmount, Is.EqualTo(0.5f).Within(0.0001f));

            var failed = new AutoEra.UI.Contracts.AutoEraUiOperationSnapshot(
                "playmode-operation", AutoEra.UI.Contracts.AutoEraUiOperationStatus.Failed, "操作失败", null, false, true, "rule-details", "rules");
            Assert.IsTrue(hub.TryApplyOperationSnapshot(hub.CurrentFormVersion, operationRequest, failed, false));
            yield return null;
            Assert.IsTrue(FindRequired(hub.transform, "Btn_RuleOperationRetry").gameObject.activeSelf);
            Assert.IsTrue(FindRequired(hub.transform, "Btn_RuleOperationDetails").gameObject.activeSelf);

            long successRequest = hub.BeginOperationRequest();
            var success = new AutoEra.UI.Contracts.AutoEraUiOperationSnapshot(
                "playmode-success", AutoEra.UI.Contracts.AutoEraUiOperationStatus.Succeeded, "已完成", null, false, false, string.Empty, "rules");
            Assert.IsTrue(hub.TryApplyOperationSnapshot(hub.CurrentFormVersion, successRequest, success, false));
            yield return null;
            Assert.IsTrue(FindRequired(hub.transform, "Art_RuleSuccessState").gameObject.activeSelf);
            yield return new WaitForSecondsRealtime(2.1f);
            Assert.IsFalse(FindRequired(hub.transform, "Art_RuleSuccessState").gameObject.activeSelf, "Success feedback should return to the stable card after about two seconds.");

            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, new BaseEventData(EventSystem.current), ExecuteEvents.cancelHandler);
            yield return WaitForForm(commandHubId, expectedLoaded: false);
            Assert.AreEqual(openingControl, EventSystem.current.currentSelectedGameObject, "Cancel should restore the control that opened the Hub.");

            GF.UI.CloseUIForm(fieldHudId);
            yield return WaitForForm(fieldHudId, expectedLoaded: false);
            Object.Destroy(openingControl);
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

        private static Transform FindRequired(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == name)
                {
                    return transforms[index];
                }
            }

            Assert.Fail("Missing required runtime UI node: " + name);
            return null;
        }
    }
}
