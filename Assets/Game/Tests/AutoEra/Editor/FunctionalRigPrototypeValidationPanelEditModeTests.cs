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
