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
            }
        }
    }
}
