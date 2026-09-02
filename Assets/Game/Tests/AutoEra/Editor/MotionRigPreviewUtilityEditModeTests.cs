using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionRigPreviewUtilityEditModeTests
    {
        [Test]
        public void PreviewUtility_ExposesRestoreMenuAndJointGizmo()
        {
            Type type = FindType();
            MethodInfo restore = type.GetMethod("RestoreSelectedRig", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo gizmo = type.GetMethod("DrawJointAxes", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(restore.GetCustomAttribute<MenuItem>(), Is.Not.Null);
            Assert.That(gizmo.GetCustomAttribute<DrawGizmo>(), Is.Not.Null);
        }

        private static Type FindType()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("AutoEra.Editor.Motion.MotionRigPreviewUtility", false);
                if (type != null) return type;
            }
            Assert.Fail("Preview utility must be loaded by the Editor.");
            return null;
        }
    }
}
