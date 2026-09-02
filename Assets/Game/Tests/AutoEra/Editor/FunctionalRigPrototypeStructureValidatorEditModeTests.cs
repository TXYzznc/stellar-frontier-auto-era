using System;
using System.Reflection;
using AutoEra.Motion;
using AutoEra.Motion.Contracts;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigPrototypeStructureValidatorEditModeTests
    {
        [Test]
        public void Validator_AcceptsGeneratedContractStructure()
        {
            GameObject prototype = CreateBuiltPrototype(out FunctionalRigContract contract, out FunctionalRigPrototypeHierarchy hierarchy);
            try
            {
                Assert.That(InvokeValidator(contract, hierarchy), Is.Empty);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prototype);
            }
        }

        [Test]
        public void Validator_ReportsMissingAnchorWrongParentDriftAndUnsupportedVisiblePart()
        {
            GameObject prototype = CreateBuiltPrototype(out FunctionalRigContract contract, out FunctionalRigPrototypeHierarchy hierarchy);
            try
            {
                UnityEngine.Object.DestroyImmediate(FindByStableId(prototype, "anchor:workpoint_main").gameObject);
                Transform geometry = FindByStableId(prototype, "geometry:slot_arm");
                geometry.SetParent(prototype.transform, false);
                geometry.localPosition = new Vector3(4f, 0f, 0f);
                GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(prototype.transform, false);

                string[] errors = InvokeValidator(contract, hierarchy);

                Assert.That(errors, Has.Some.Contains("Missing generated object: anchor:workpoint_main"));
                Assert.That(errors, Has.Some.Contains("visual geometry has invalid parent: slot_arm"));
                Assert.That(errors, Has.Some.Contains("visual geometry center drifted: slot_arm"));
                Assert.That(errors, Has.Some.Contains("Visible renderer lacks a declared visual-slot support path"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prototype);
            }
        }

        private static GameObject CreateBuiltPrototype(out FunctionalRigContract contract, out FunctionalRigPrototypeHierarchy hierarchy)
        {
            GameObject prototype = new GameObject("Prototype");
            Transform logicRoot = CreateChild(prototype.transform, "LogicRoot");
            Transform rigRoot = CreateChild(prototype.transform, "RigRoot");
            Transform visualRoot = CreateChild(rigRoot, "VisualRoot");
            Transform authorityCollisionRoot = CreateChild(prototype.transform, "AuthorityCollisionRoot");
            authorityCollisionRoot.gameObject.AddComponent<BoxCollider>();
            hierarchy = prototype.AddComponent<FunctionalRigPrototypeHierarchy>();
            hierarchy.Configure(logicRoot, rigRoot, visualRoot, authorityCollisionRoot);
            contract = CreateValidContract();
            InvokeEditorBuilder(contract, hierarchy);
            return prototype;
        }

        private static void InvokeEditorBuilder(FunctionalRigContract contract, FunctionalRigPrototypeHierarchy hierarchy)
        {
            InvokeEditorMethod("AutoEra.Editor.Motion.FunctionalRigPrototypeBuilder", "Build", contract, hierarchy);
        }

        private static string[] InvokeValidator(FunctionalRigContract contract, FunctionalRigPrototypeHierarchy hierarchy)
        {
            return (string[])InvokeEditorMethod("AutoEra.Editor.Motion.FunctionalRigPrototypeStructureValidator", "Validate", contract, hierarchy);
        }

        private static object InvokeEditorMethod(string typeName, string methodName, FunctionalRigContract contract, FunctionalRigPrototypeHierarchy hierarchy)
        {
            Type type = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName, false);
                if (type != null)
                {
                    break;
                }
            }

            Assert.That(type, Is.Not.Null, typeName + " must be loaded by the Editor.");
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, typeName + "." + methodName + " must be public and static.");
            try
            {
                return method.Invoke(null, new object[] { contract, hierarchy });
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
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
                Joints = new[]
                {
                    new FunctionalRigJoint
                    {
                        StableId = "arm_yaw", ObjectPath = "RigRoot/arm_yaw", Channel = "rotation",
                        LocalAxis = new ContractVector3 { Y = 1f }, MinimumValue = -90f, MaximumValue = 90f
                    }
                },
                Anchors = new[] { new FunctionalRigAnchor { StableId = "workpoint_main", Kind = "workpoint", ParentStableId = "arm_yaw" } },
                ClearanceVolumes = new[] { new FunctionalRigVolume { StableId = "keepout_arm", Kind = "keepout", ParentStableId = "arm_yaw", Size = new ContractVector3 { X = 1f, Y = 1f, Z = 1f } } },
                CollisionEnvelopes = new[] { new FunctionalRigVolume { StableId = "collision_chassis", Kind = "collision", Size = new ContractVector3 { X = 2f, Y = 1f, Z = 3f } } },
                VisualSlots = new[]
                {
                    new FunctionalRigVisualSlot
                    {
                        StableId = "slot_arm", ParentStableId = "arm_yaw", ObjectPath = "RigRoot/ArmYaw/VisualSlot",
                        ExpectedBounds = new ContractBounds { Size = new ContractVector3 { X = 1f, Y = 1f, Z = 1f } }
                    }
                }
            };
        }
    }
}
