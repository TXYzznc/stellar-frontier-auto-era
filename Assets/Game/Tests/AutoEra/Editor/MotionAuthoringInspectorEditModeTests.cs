using System;
using System.Reflection;
using AutoEra.Motion;
using NUnit.Framework;
using UnityEditor;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionAuthoringInspectorEditModeTests
    {
        [TestCase("AutoEra.Editor.Motion.MotionRigInspector", typeof(MotionRig))]
        [TestCase("AutoEra.Editor.Motion.MotionGraphAssetInspector", typeof(MotionGraphAsset))]
        public void AuthoringInspector_BindsExpectedTargetType(string inspectorTypeName, Type targetType)
        {
            Type inspectorType = FindType(inspectorTypeName);
            CustomEditor attribute = (CustomEditor)Attribute.GetCustomAttribute(inspectorType, typeof(CustomEditor));
            Assert.That(attribute, Is.Not.Null);
            FieldInfo inspectedTypeField = typeof(CustomEditor).GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(inspectedTypeField, Is.Not.Null);
            Assert.That(inspectedTypeField.GetValue(attribute), Is.EqualTo(targetType));
        }

        private static Type FindType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName, false);
                if (type != null) return type;
            }
            Assert.Fail("Could not load authoring inspector: " + typeName);
            return null;
        }
    }
}
