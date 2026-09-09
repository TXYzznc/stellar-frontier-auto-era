using NUnit.Framework;
using AutoEra.UI;
using AutoEra.UI.Contracts;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraUiIntentRouterEditModeTests
    {
        [Test]
        public void Dispatch_StopsAtTopmostFormThatConsumesIntent()
        {
            var router = new AutoEraUiIntentRouter();
            GameObject lowerObject = new GameObject("lower");
            GameObject upperObject = new GameObject("upper");
            try
            {
                var lower = lowerObject.AddComponent<TestUiForm>();
                var upper = upperObject.AddComponent<TestUiForm>();
                lower.SetConsume(true);
                upper.SetConsume(true);
                router.Register(lower);
                router.Register(upper);

                bool consumed = router.Dispatch(AutoEraUiIntent.NavigateNext);

                Assert.IsTrue(consumed);
                Assert.AreEqual(0, lower.IntentCount);
                Assert.AreEqual(1, upper.IntentCount);
            }
            finally
            {
                Object.DestroyImmediate(lowerObject);
                Object.DestroyImmediate(upperObject);
            }
        }

        [Test]
        public void Dispatch_SkipsTopmostFormWhenItDoesNotConsumeIntent()
        {
            var router = new AutoEraUiIntentRouter();
            GameObject lowerObject = new GameObject("lower");
            GameObject upperObject = new GameObject("upper");
            try
            {
                var lower = lowerObject.AddComponent<TestUiForm>();
                var upper = upperObject.AddComponent<TestUiForm>();
                lower.SetConsume(true);
                upper.SetConsume(false);
                router.Register(lower);
                router.Register(upper);

                bool consumed = router.Dispatch(AutoEraUiIntent.Confirm);

                Assert.IsTrue(consumed);
                Assert.AreEqual(1, lower.IntentCount);
                Assert.AreEqual(1, upper.IntentCount);
            }
            finally
            {
                Object.DestroyImmediate(lowerObject);
                Object.DestroyImmediate(upperObject);
            }
        }

        private sealed class TestUiForm : AutoEraUiFormBase
        {
            private bool _consume;

            public int IntentCount { get; private set; }

            public void SetConsume(bool consume)
            {
                _consume = consume;
            }

            protected override bool OnAutoEraIntent(AutoEraUiIntent intent)
            {
                IntentCount++;
                return _consume;
            }

            protected override void OnOperationPresentationChanged(AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
        }
    }
}
