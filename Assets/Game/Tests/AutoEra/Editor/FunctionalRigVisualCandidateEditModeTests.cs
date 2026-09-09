using AutoEra.Motion;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigVisualCandidateEditModeTests
    {
        [Test]
        public void FormalEntityVariant_PreservesFunctionalHierarchyAndContractBinding()
        {
            const string candidatePath = "Assets/Game/Prefabs/Entity/Machines/Carriers/WheeledCarrier.prefab";
            GameObject candidate = AssetDatabase.LoadAssetAtPath<GameObject>(candidatePath);
            Assert.That(candidate, Is.Not.Null);

            FunctionalRigPrototypeHierarchy hierarchy = candidate.GetComponent<FunctionalRigPrototypeHierarchy>();
            Assert.That(hierarchy, Is.Not.Null);
            Assert.That(hierarchy.LogicRoot.parent, Is.EqualTo(candidate.transform));
            Assert.That(hierarchy.RigRoot.parent, Is.EqualTo(candidate.transform));
            Assert.That(hierarchy.VisualRoot.parent, Is.EqualTo(hierarchy.RigRoot));
            Assert.That(hierarchy.AuthorityCollisionRoot.parent, Is.EqualTo(candidate.transform));
        }
    }
}
