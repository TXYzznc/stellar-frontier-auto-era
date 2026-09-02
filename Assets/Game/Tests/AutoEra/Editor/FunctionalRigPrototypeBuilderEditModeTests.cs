using System;
using System.Reflection;
using AutoEra.Motion;
using AutoEra.Motion.Contracts;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigPrototypeBuilderEditModeTests
    {
        [Test]
        public void Builder_ReusesStableObjectsAndUpdatesDeclaredVisualBounds()
        {
            GameObject prototype = CreatePrototype(out FunctionalRigPrototypeHierarchy hierarchy);
            try
            {
                FunctionalRigContract contract = CreateValidContract();
                InvokeEditorBuilder(contract, hierarchy);

                Transform joint = FindByStableId(prototype, "joint:arm_yaw");
                Transform slot = FindByStableId(prototype, "visual-slot:slot_arm");
                Transform geometry = FindByStableId(prototype, "geometry:slot_arm");
                Transform collision = FindByStableId(prototype, "collision:collision_chassis");
                Assert.That(slot.parent, Is.EqualTo(joint));
                Assert.That(geometry.parent, Is.EqualTo(slot));
                Assert.That(collision.parent, Is.EqualTo(hierarchy.AuthorityCollisionRoot));
                Assert.That(geometry.GetComponent<Collider>(), Is.Null);

                contract.VisualSlots[0].ExpectedBounds.Size = new ContractVector3 { X = 2f, Y = 3f, Z = 4f };
                InvokeEditorBuilder(contract, hierarchy);

                Assert.That(FindByStableId(prototype, "joint:arm_yaw"), Is.EqualTo(joint));
                Assert.That(FindByStableId(prototype, "visual-slot:slot_arm"), Is.EqualTo(slot));
                Assert.That(FindByStableId(prototype, "geometry:slot_arm"), Is.EqualTo(geometry));
                Assert.That(geometry.localScale, Is.EqualTo(new Vector3(2f, 3f, 4f)));
                Assert.That(prototype.GetComponentsInChildren<FunctionalRigPrototypeStableId>(true), Has.Length.EqualTo(6));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prototype);
            }
        }

        private static void InvokeEditorBuilder(FunctionalRigContract contract, FunctionalRigPrototypeHierarchy hierarchy)
        {
            Type builderType = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                builderType = assembly.GetType("AutoEra.Editor.Motion.FunctionalRigPrototypeBuilder", false);
                if (builderType != null)
                {
                    break;
                }
            }

            Assert.That(builderType, Is.Not.Null, "The Editor Builder assembly must be loaded for this EditMode test.");
            MethodInfo build = builderType.GetMethod("Build", BindingFlags.Public | BindingFlags.Static);
            Assert.That(build, Is.Not.Null, "FunctionalRigPrototypeBuilder.Build must be public and static.");
            try
            {
                build.Invoke(null, new object[] { contract, hierarchy });
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        private static GameObject CreatePrototype(out FunctionalRigPrototypeHierarchy hierarchy)
        {
            GameObject prototype = new GameObject("Prototype");
            Transform logicRoot = CreateChild(prototype.transform, "LogicRoot");
            Transform rigRoot = CreateChild(prototype.transform, "RigRoot");
            Transform visualRoot = CreateChild(rigRoot, "VisualRoot");
            Transform authorityCollisionRoot = CreateChild(prototype.transform, "AuthorityCollisionRoot");
            authorityCollisionRoot.gameObject.AddComponent<BoxCollider>();
            hierarchy = prototype.AddComponent<FunctionalRigPrototypeHierarchy>();
            hierarchy.Configure(logicRoot, rigRoot, visualRoot, authorityCollisionRoot);
            return prototype;
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static Transform FindByStableId(GameObject root, string stableId)
        {
            foreach (FunctionalRigPrototypeStableId identity in root.GetComponentsInChildren<FunctionalRigPrototypeStableId>(true))
            {
                if (identity.StableId == stableId)
                {
                    return identity.transform;
                }
            }

            Assert.Fail("Could not find generated object with stable ID: " + stableId);
            return null;
        }

        private static FunctionalRigContract CreateValidContract()
        {
            return new FunctionalRigContract
            {
                ContractId = "prototype_carrier",
                AssetFamilyId = "wheeled_carrier",
                ContractVersion = "1.0.0",
                OverallBounds = new ContractBounds { Size = new ContractVector3 { X = 2f, Y = 1f, Z = 3f } },
                Compatibility = new FunctionalRigCompatibility
                {
                    MinimumConsumerSchemaVersion = 1,
                    MaximumConsumerSchemaVersion = 1,
                    VisualReplacementCompatibilityId = "carrier_v1"
                },
                Joints = new[] { CreateJoint("arm_yaw", string.Empty) },
                Anchors = new[]
                {
                    new FunctionalRigAnchor { StableId = "workpoint_main", Kind = "workpoint", ParentStableId = "arm_yaw" }
                },
                ClearanceVolumes = new[]
                {
                    new FunctionalRigVolume { StableId = "keepout_arm", Kind = "keepout", ParentStableId = "arm_yaw", Size = new ContractVector3 { X = 1f, Y = 1f, Z = 1f } }
                },
                CollisionEnvelopes = new[]
                {
                    new FunctionalRigVolume { StableId = "collision_chassis", Kind = "collision", ParentStableId = string.Empty, Size = new ContractVector3 { X = 2f, Y = 1f, Z = 3f } }
                },
                VisualSlots = new[]
                {
                    new FunctionalRigVisualSlot
                    {
                        StableId = "slot_arm",
                        ParentStableId = "arm_yaw",
                        ObjectPath = "RigRoot/ArmYaw/VisualSlot",
                        ExpectedBounds = new ContractBounds { Size = new ContractVector3 { X = 1f, Y = 1f, Z = 1f } }
                    }
                }
            };
        }

        private static FunctionalRigJoint CreateJoint(string stableId, string parentStableId)
        {
            return new FunctionalRigJoint
            {
                StableId = stableId,
                ParentStableId = parentStableId,
                ObjectPath = "RigRoot/" + stableId,
                Channel = "rotation",
                LocalAxis = new ContractVector3 { Y = 1f },
                MinimumValue = -90f,
                MaximumValue = 90f,
                BindPose = new ContractPose(),
                SafePose = new ContractPose()
            };
        }
    }
}
