using AutoEra.UI.Contracts;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraUiOperationContractsEditModeTests
    {
        [Test]
        public void Presentation_UsesOnlyAuthoritativeTerminalStateAndTrustedProgress()
        {
            var pending = new AutoEraUiOperationSnapshot(
                "request-1",
                AutoEraUiOperationStatus.InProgress,
                "正在执行",
                0.5f,
                true,
                false,
                "details-1",
                "source-1");

            AutoEraUiOperationPresentation pendingPresentation = AutoEraUiOperationPresentation.Create(pending, true);
            Assert.That(pendingPresentation.ShowSpinner, Is.True);
            Assert.That(pendingPresentation.ShowProgress, Is.True);
            Assert.That(pendingPresentation.ShowLongWaitHint, Is.True);
            Assert.That(pendingPresentation.ShowCancel, Is.True);
            Assert.That(pendingPresentation.ShowRetry, Is.False);

            var failed = new AutoEraUiOperationSnapshot(
                "request-1",
                AutoEraUiOperationStatus.Failed,
                "操作失败",
                0.8f,
                true,
                true,
                "details-1",
                "source-1");

            AutoEraUiOperationPresentation failedPresentation = AutoEraUiOperationPresentation.Create(failed, true);
            Assert.That(failedPresentation.ShowSpinner, Is.False);
            Assert.That(failedPresentation.ShowLongWaitHint, Is.False);
            Assert.That(failedPresentation.ShowRetry, Is.True);
            Assert.That(failedPresentation.RetryRequiresConfirmation, Is.True);
        }

        [Test]
        public void RequestGate_RejectsLateCallbacksAfterCloseOrNewRequest()
        {
            var gate = new AutoEraUiRequestVersionGate();
            gate.Open();
            long formVersion = gate.FormVersion;
            long firstRequest = gate.BeginRequest();
            Assert.That(gate.Accepts(formVersion, firstRequest), Is.True);

            long secondRequest = gate.BeginRequest();
            Assert.That(gate.Accepts(formVersion, firstRequest), Is.False);
            Assert.That(gate.Accepts(formVersion, secondRequest), Is.True);

            gate.Close();
            Assert.That(gate.Accepts(formVersion, secondRequest), Is.False);
        }

        [Test]
        public void Snapshot_DropsOutOfRangeProgressInsteadOfInventingAProgressBar()
        {
            var snapshot = new AutoEraUiOperationSnapshot(
                "request-2",
                AutoEraUiOperationStatus.InProgress,
                "仍在处理中",
                1.1f,
                false,
                false,
                string.Empty,
                "source-2");

            AutoEraUiOperationPresentation presentation = AutoEraUiOperationPresentation.Create(snapshot, true);
            Assert.That(snapshot.TrustedProgress, Is.Null);
            Assert.That(presentation.ShowProgress, Is.False);
            Assert.That(presentation.ShowLongWaitHint, Is.True);
        }

        [Test]
        public void HubPageSelection_CyclesOnlyAcrossTheFiveFrozenHubPages()
        {
            var selection = new AutoEra.UI.AutoEraHubPageSelection(AutoEra.UI.AutoEraHubPage.Overview);
            Assert.That(selection.Move(-1), Is.EqualTo(AutoEra.UI.AutoEraHubPage.Statistics));
            Assert.That(selection.Move(1), Is.EqualTo(AutoEra.UI.AutoEraHubPage.Overview));
            Assert.That(selection.Select(AutoEra.UI.AutoEraHubPage.Rules), Is.True);
            Assert.That(selection.Select(AutoEra.UI.AutoEraHubPage.Rules), Is.False);
        }

        [Test]
        public void ConfirmationDescriptionMissing_DisablesDangerAndKeepsCancelAsInitialFocus()
        {
            var missing = AutoEraConfirmationDescriptionResolution.Missing("rules.disable", "affectedTaskCount");
            var presentation = new AutoEraDangerConfirmationPresentation(missing);

            Assert.That(presentation.IsDangerActionEnabled, Is.False);
            Assert.That(presentation.IsCancelEnabled, Is.True);
            Assert.That(presentation.InitialFocus, Is.EqualTo(AutoEraConfirmationInitialFocus.Cancel));
            Assert.That(missing.ErrorTitle, Is.EqualTo("确认描述配置缺失"));
            StringAssert.Contains("affectedTaskCount", missing.BuildConsoleDiagnostic());
            StringAssert.StartsWith("确认描述配置缺失：rules.disable\n", missing.ErrorDetail);
            Assert.That(missing.ErrorDetail.Split('\n').Length, Is.EqualTo(2));

            var resourceMissingWithoutParameter = AutoEraConfirmationDescriptionResolution.Missing("rules.disable", string.Empty);
            Assert.That(resourceMissingWithoutParameter.IsValid, Is.False);
        }

        [Test]
        public void HoldToConfirm_CompletesOnlyAfterOnePointTwoSecondsAndReleaseCancels()
        {
            var tracker = new AutoEraHoldToConfirmTracker();
            tracker.Begin();

            Assert.That(tracker.Advance(1.19f), Is.False);
            Assert.That(tracker.Progress, Is.EqualTo(1.19f / 1.2f).Within(0.0001f));
            tracker.Release();
            Assert.That(tracker.Progress, Is.EqualTo(0f));

            tracker.Begin();
            Assert.That(tracker.Advance(1.2f), Is.True);
            Assert.That(tracker.IsCompleted, Is.True);
        }

        [Test]
        public void HoldToConfirmView_OnlyRaisesConfirmationAfterTheFullHold()
        {
            var gameObject = new UnityEngine.GameObject("hold-to-confirm");
            try
            {
                var view = gameObject.AddComponent<AutoEra.UI.AutoEraHoldToConfirmView>();
                int confirmations = 0;
                view.Confirmed += () => confirmations++;

                view.BeginHold();
                view.AdvanceForPreview(1.19f);
                Assert.That(confirmations, Is.EqualTo(0));
                view.CancelHold();
                Assert.That(view.Progress, Is.EqualTo(0f));

                view.BeginHold();
                view.AdvanceForPreview(1.2f);
                Assert.That(confirmations, Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }
    }
}
