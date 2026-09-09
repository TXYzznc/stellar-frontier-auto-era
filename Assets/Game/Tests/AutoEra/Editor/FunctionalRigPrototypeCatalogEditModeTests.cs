using AutoEra.Motion;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigPrototypeCatalogEditModeTests
    {
        [Test]
        public void Catalog_ContainsAllRepresentativePrototypePrefabs()
        {
            foreach (string familyId in FunctionalRigPrototypeCatalog.AssetFamilyIds)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/FunctionalPrototypes/Catalog/" + familyId + ".prefab");
                Assert.That(prefab, Is.Not.Null, familyId);
                FunctionalRigPrototypeHierarchy hierarchy = prefab.GetComponent<FunctionalRigPrototypeHierarchy>();
                Assert.That(hierarchy, Is.Not.Null, familyId);
                Assert.That(hierarchy.TryValidate(out string error), Is.True, error);
                MotionRig rig = prefab.GetComponent<MotionRig>();
                Assert.That(rig, Is.Not.Null, familyId + " requires a MotionRig for the unified preview tool.");
                Assert.That(rig.TryValidate(out string rigError), Is.True, rigError);
                if (familyId == "conveyor")
                {
                    ConveyorLoopRigPreview preview = prefab.GetComponentInChildren<ConveyorLoopRigPreview>(true);
                    Assert.That(preview, Is.Not.Null,
                        "The conveyor prefab requires a self-contained visual loop presenter.");
                    MeshFilter beltMesh = preview.GetComponent<MeshFilter>();
                    Assert.That(beltMesh, Is.Not.Null);
                    Assert.That(beltMesh.sharedMesh, Is.Not.Null);
                    Assert.That(beltMesh.sharedMesh.vertexCount, Is.GreaterThan(500),
                        "The conveyor requires one detailed continuous belt mesh, not two planes or child cubes.");
                    Assert.That(beltMesh.sharedMesh.subMeshCount, Is.EqualTo(2),
                        "The continuous belt mesh requires a separate in-mesh motion-marker submesh.");
                    Assert.That(preview.GetComponent<MeshRenderer>().sharedMaterials, Has.Length.EqualTo(2),
                        "The belt requires a distinct marker material so its physical travel is visually readable.");
                    foreach (Transform node in prefab.GetComponentsInChildren<Transform>(true))
                    {
                        Assert.That(node.name.StartsWith("BeltTread_"), Is.False,
                            "The conveyor belt must remain one mesh rather than independent tread GameObjects.");
                    }
                }
            }
        }
    }
}
