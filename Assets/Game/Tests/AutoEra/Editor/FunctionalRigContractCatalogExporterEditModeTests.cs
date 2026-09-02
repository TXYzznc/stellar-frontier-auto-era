using System;
using System.Reflection;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigContractCatalogExporterEditModeTests
    {
        [Test]
        public void CatalogExporter_BuildsVersionedFingerprintManifestForEveryFamily()
        {
            Type exporterType = FindExporterType();
            MethodInfo buildManifest = exporterType.GetMethod("BuildManifestJson", BindingFlags.Public | BindingFlags.Static);
            string json = (string)buildManifest.Invoke(null, null);
            Assert.That(Regex.Matches(json, "\\\"assetFamilyId\\\"").Count, Is.EqualTo(6));
            Assert.That(Regex.Matches(json, "\\\"contractVersion\\\":\\\"1.0.0\\\"").Count, Is.EqualTo(6));
            MatchCollection fingerprints = Regex.Matches(json, "\\\"contentFingerprint\\\":\\\"([0-9a-f]{64})\\\"");
            Assert.That(fingerprints.Count, Is.EqualTo(6));
            foreach (Match fingerprint in fingerprints)
            {
                Assert.That(fingerprint.Groups[1].Value, Has.Length.EqualTo(64));
            }
        }

        private static Type FindExporterType()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType("AutoEra.Editor.Motion.FunctionalRigContractCatalogExporter", false);
                if (type != null)
                {
                    return type;
                }
            }

            Assert.Fail("FunctionalRigContractCatalogExporter must be loaded by the Editor.");
            return null;
        }
    }
}
