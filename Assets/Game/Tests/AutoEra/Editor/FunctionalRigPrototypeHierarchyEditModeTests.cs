using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigPrototypeHierarchyEditModeTests
    {
        [Test]
        public void VisualJointMotion_DoesNotMoveLogicOrAuthorityCollisionRoots()
        {
            GameObject prototype = new GameObject("Prototype");
            try
            {
                Transform logicRoot = CreateChild(prototype.transform, "LogicRoot");
                Transform rigRoot = CreateChild(prototype.transform, "RigRoot");
                Transform visualRoot = CreateChild(rigRoot, "VisualRoot");
                Transform visualJoint = CreateChild(visualRoot, "Joint_Test");
                Transform authorityCollisionRoot = CreateChild(prototype.transform, "AuthorityCollisionRoot");
                authorityCollisionRoot.gameObject.AddComponent<BoxCollider>();

                FunctionalRigPrototypeHierarchy hierarchy = prototype.AddComponent<FunctionalRigPrototypeHierarchy>();
                hierarchy.Configure(logicRoot, rigRoot, visualRoot, authorityCollisionRoot);
                Vector3 logicPosition = logicRoot.position;
                Vector3 collisionPosition = authorityCollisionRoot.position;

                visualJoint.localPosition = new Vector3(2f, 3f, 4f);

                Assert.That(hierarchy.TryValidate(out string error), Is.True, error);
                Assert.That(logicRoot.position, Is.EqualTo(logicPosition));
                Assert.That(authorityCollisionRoot.position, Is.EqualTo(collisionPosition));
            }
            finally
            {
                Object.DestroyImmediate(prototype);
            }
        }

        [Test]
        public void Validator_RejectsWhenLogicRootIsMovedInsideVisualRig()
        {
            GameObject prototype = new GameObject("Prototype");
            try
            {
                Transform rigRoot = CreateChild(prototype.transform, "RigRoot");
                Transform visualRoot = CreateChild(rigRoot, "VisualRoot");
                Transform logicRoot = CreateChild(rigRoot, "LogicRoot");
                Transform authorityCollisionRoot = CreateChild(prototype.transform, "AuthorityCollisionRoot");
                authorityCollisionRoot.gameObject.AddComponent<BoxCollider>();

                FunctionalRigPrototypeHierarchy hierarchy = prototype.AddComponent<FunctionalRigPrototypeHierarchy>();
                hierarchy.Configure(logicRoot, rigRoot, visualRoot, authorityCollisionRoot);

                Assert.That(hierarchy.TryValidate(out string error), Is.False);
                Assert.That(error, Does.Contain("direct siblings"));
            }
            finally
            {
                Object.DestroyImmediate(prototype);
            }
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }
    }
}
