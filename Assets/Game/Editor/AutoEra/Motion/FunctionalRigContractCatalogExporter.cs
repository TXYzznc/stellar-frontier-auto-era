using System.IO;
using AutoEra.Motion;
using AutoEra.Motion.Contracts;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace AutoEra.Editor.Motion
{
    internal static class FunctionalRigContractCatalogExporter
    {
        internal const string ExportFolder = "Assets/Game/Config/FunctionalRigContracts/Exports";

        [MenuItem("AutoEra/Functional Prototypes/Export Contract Catalog")]
        private static void ExportCatalog()
        {
            Directory.CreateDirectory(ExportFolder);
            var manifest = new JArray();
            foreach (string familyId in FunctionalRigPrototypeCatalog.AssetFamilyIds)
            {
                FunctionalRigContract contract = FunctionalRigPrototypeCatalog.Create(familyId);
                string json = FunctionalRigContractJson.SerializeNormalized(contract, out string fingerprint);
                File.WriteAllText(Path.Combine(ExportFolder, familyId + ".json"), json);
                manifest.Add(new JObject
                {
                    ["assetFamilyId"] = familyId,
                    ["contractId"] = contract.ContractId,
                    ["contractVersion"] = contract.ContractVersion,
                    ["contentFingerprint"] = fingerprint
                });
            }

            File.WriteAllText(Path.Combine(ExportFolder, "functional-rig-contract-manifest.json"), manifest.ToString(Newtonsoft.Json.Formatting.Indented));
            AssetDatabase.Refresh();
        }

        public static string BuildManifestJson()
        {
            var manifest = new JArray();
            foreach (string familyId in FunctionalRigPrototypeCatalog.AssetFamilyIds)
            {
                FunctionalRigContract contract = FunctionalRigPrototypeCatalog.Create(familyId);
                FunctionalRigContractJson.SerializeNormalized(contract, out string fingerprint);
                manifest.Add(new JObject
                {
                    ["assetFamilyId"] = familyId,
                    ["contractId"] = contract.ContractId,
                    ["contractVersion"] = contract.ContractVersion,
                    ["contentFingerprint"] = fingerprint
                });
            }

            return manifest.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
