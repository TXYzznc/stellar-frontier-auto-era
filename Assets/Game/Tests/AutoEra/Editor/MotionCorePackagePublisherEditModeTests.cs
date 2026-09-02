using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionCorePackagePublisherEditModeTests
    {
        [Test]
        public void Publisher_ProducesSelfContainedPackageWithFixedSourceMeta()
        {
            Type publisher = FindPublisher();
            string root = (string)publisher.GetMethod("Publish", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            Assert.That(File.Exists(Path.Combine(root, "package.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "Runtime/AutoEra.MotionCore.Runtime.asmdef")), Is.True);
            Assert.That(File.Exists(Path.Combine(root, "Editor/AutoEra.MotionCore.Editor.asmdef")), Is.True);
            object[] validationArguments = { root, null };
            bool isValid = (bool)publisher.GetMethod("ValidatePublishedPackage", BindingFlags.Public | BindingFlags.Static).Invoke(null, validationArguments);
            Assert.That(isValid, Is.True, validationArguments[1] as string);
            Assert.That(Directory.GetFiles(root, "*Adapter*", SearchOption.AllDirectories), Is.Empty);
            Assert.That(Directory.GetFiles(root, "*.unity", SearchOption.AllDirectories), Is.Empty);
            Assert.That(Directory.GetFiles(root, "*.xlsx", SearchOption.AllDirectories), Is.Empty);
        }

        private static Type FindPublisher()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type publisher = assembly.GetType("AutoEra.Editor.Motion.MotionCorePackagePublisher", false);
                if (publisher != null) return publisher;
            }

            Assert.Fail("MotionCorePackagePublisher must be available from the Editor assembly.");
            return null;
        }
    }
}
