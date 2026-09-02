using System;
using System.Reflection;
using AutoEra.Motion.Contracts;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigPrototypeValidationPanelEditModeTests
    {
        [Test]
        public void ValidationPanel_UsesContractCodecForInvalidJson()
        {
            Type panelType = FindPanelType();
            MethodInfo validate = panelType.GetMethod("ValidateContractJson", BindingFlags.Public | BindingFlags.Static);
            Assert.That(validate, Is.Not.Null);

            object[] arguments = { "{ }", null };
            string[] errors = (string[])validate.Invoke(null, arguments);

            Assert.That(errors, Is.Not.Empty);
            Assert.That(arguments[1], Is.Null);
            Assert.That(errors[0], Does.Contain("assetFamilyId"));
        }

        [Test]
        public void ValidationPanel_ControlStateSupportsReplayInterruptionAndBaselineReset()
        {
            Type panelType = FindPanelType();
            Type capabilityType = panelType.Assembly.GetType("AutoEra.Editor.Motion.MotionValidationCapability", true);
            Type controlType = panelType.Assembly.GetType("AutoEra.Editor.Motion.MotionValidationControl", true);
            object fourWheel = System.Enum.Parse(capabilityType, "FourWheel");
            object play = System.Enum.Parse(controlType, "Play");
            object interrupt = System.Enum.Parse(controlType, "Interrupt");
            object recover = System.Enum.Parse(controlType, "Recover");
            object reset = System.Enum.Parse(controlType, "Reset");
            object state = panelType.GetMethod("CreateControlState", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { fourWheel });
            MethodInfo apply = panelType.GetMethod("ApplyControl", BindingFlags.Public | BindingFlags.Static);

            state = apply.Invoke(null, new[] { state, play });
            state = apply.Invoke(null, new[] { state, interrupt });
            state = apply.Invoke(null, new[] { state, recover });
            state = apply.Invoke(null, new[] { state, reset });

            Type stateType = state.GetType();
            Assert.That((bool)stateType.GetProperty("IsPlaying").GetValue(state), Is.False);
            Assert.That((bool)stateType.GetProperty("IsInterrupted").GetValue(state), Is.False);
            Assert.That((float)stateType.GetProperty("TestProgress").GetValue(state), Is.Zero);
        }

        private static Type FindPanelType()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type panelType = assembly.GetType("AutoEra.Editor.Motion.FunctionalRigPrototypeValidationPanel", false);
                if (panelType != null)
                {
                    return panelType;
                }
            }

            Assert.Fail("FunctionalRigPrototypeValidationPanel must be loaded by the Editor.");
            return null;
        }
    }
}
