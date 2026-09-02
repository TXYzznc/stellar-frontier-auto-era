using System;
using System.Collections.Generic;
using AutoEra.Motion;
using AutoEra.Motion.Contracts;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    /// <summary>
    /// Validates that a generated prototype still matches its structural contract.
    /// It intentionally reports every detected violation so the fixed validation panel can display it later.
    /// </summary>
    public static class FunctionalRigPrototypeStructureValidator
    {
        private const float TransformTolerance = 0.0001f;

        public static string[] Validate(FunctionalRigContract contract, FunctionalRigPrototypeHierarchy hierarchy)
        {
            var errors = new List<string>();
            if (contract == null)
            {
                errors.Add("FunctionalRigContract is required.");
                return errors.ToArray();
            }

            var contractErrors = new List<string>();
            if (!FunctionalRigContractValidator.TryValidate(contract, contractErrors))
            {
                errors.AddRange(contractErrors);
                return errors.ToArray();
            }

            string hierarchyError = null;
            if (hierarchy == null || !hierarchy.TryValidate(out hierarchyError))
            {
                errors.Add(hierarchy == null ? "FunctionalRigPrototypeHierarchy is required." : hierarchyError);
                return errors.ToArray();
            }

            Dictionary<string, Transform> generated = CollectGenerated(hierarchy.transform, errors);
            var joints = new Dictionary<string, Transform>(StringComparer.Ordinal);
            ValidateJoints(contract.Joints, hierarchy.VisualRoot, generated, joints, errors);
            ValidateAnchors(contract.Anchors, hierarchy.RigRoot, joints, generated, errors);
            ValidateClearanceVolumes(contract.ClearanceVolumes, hierarchy.RigRoot, joints, generated, errors);
            ValidateCollisionEnvelopes(contract.CollisionEnvelopes, hierarchy.AuthorityCollisionRoot, generated, errors);
            ValidateVisualSlots(contract.VisualSlots, hierarchy.VisualRoot, joints, generated, errors);
            ValidateNoUnexpectedGeneratedObjects(contract, generated, errors);
            ValidateVisibleSupportPaths(hierarchy, errors);
            return errors.ToArray();
        }

        private static Dictionary<string, Transform> CollectGenerated(Transform root, List<string> errors)
        {
            var result = new Dictionary<string, Transform>(StringComparer.Ordinal);
            foreach (FunctionalRigPrototypeStableId identity in root.GetComponentsInChildren<FunctionalRigPrototypeStableId>(true))
            {
                if (string.IsNullOrEmpty(identity.StableId))
                {
                    errors.Add("Generated object has an empty stable ID: " + identity.name);
                }
                else if (result.ContainsKey(identity.StableId))
                {
                    errors.Add("Duplicate generated stable ID: " + identity.StableId);
                }
                else
                {
                    result.Add(identity.StableId, identity.transform);
                }
            }

            return result;
        }

        private static void ValidateJoints(FunctionalRigJoint[] definitions, Transform visualRoot, Dictionary<string, Transform> generated, Dictionary<string, Transform> joints, List<string> errors)
        {
            var pending = new List<FunctionalRigJoint>(definitions);
            while (pending.Count > 0)
            {
                bool validatedOne = false;
                for (int index = pending.Count - 1; index >= 0; index--)
                {
                    FunctionalRigJoint definition = pending[index];
                    Transform expectedParent;
                    if (string.IsNullOrEmpty(definition.ParentStableId))
                    {
                        expectedParent = visualRoot;
                    }
                    else if (!joints.TryGetValue(definition.ParentStableId, out expectedParent))
                    {
                        continue;
                    }

                    Transform joint = Get(generated, "joint:" + definition.StableId, errors);
                    if (joint != null)
                    {
                        ValidateParent(joint, expectedParent, "joint", definition.StableId, errors);
                        ValidatePose(joint, definition.BindPose, "joint", definition.StableId, errors);
                        joints[definition.StableId] = joint;
                    }

                    pending.RemoveAt(index);
                    validatedOne = true;
                }

                if (!validatedOne)
                {
                    errors.Add("Joint hierarchy has an unresolved parent reference.");
                    return;
                }
            }
        }

        private static void ValidateAnchors(FunctionalRigAnchor[] definitions, Transform fallbackParent, Dictionary<string, Transform> joints, Dictionary<string, Transform> generated, List<string> errors)
        {
            foreach (FunctionalRigAnchor definition in definitions)
            {
                Transform anchor = Get(generated, "anchor:" + definition.StableId, errors);
                if (anchor == null)
                {
                    continue;
                }

                ValidateParent(anchor, GetJointParent(definition.ParentStableId, joints, fallbackParent), "anchor", definition.StableId, errors);
                ValidatePose(anchor, definition.LocalPose, "anchor", definition.StableId, errors);
            }
        }

        private static void ValidateClearanceVolumes(FunctionalRigVolume[] definitions, Transform fallbackParent, Dictionary<string, Transform> joints, Dictionary<string, Transform> generated, List<string> errors)
        {
            foreach (FunctionalRigVolume definition in definitions)
            {
                Transform volume = Get(generated, "clearance:" + definition.StableId, errors);
                if (volume == null)
                {
                    continue;
                }

                ValidateParent(volume, GetJointParent(definition.ParentStableId, joints, fallbackParent), "clearance", definition.StableId, errors);
                ValidatePose(volume, definition.LocalPose, "clearance", definition.StableId, errors);
                ValidateVector(volume.localScale, ToVector3(definition.Size), "clearance size", definition.StableId, errors);
            }
        }

        private static void ValidateCollisionEnvelopes(FunctionalRigVolume[] definitions, Transform authorityCollisionRoot, Dictionary<string, Transform> generated, List<string> errors)
        {
            foreach (FunctionalRigVolume definition in definitions)
            {
                Transform envelope = Get(generated, "collision:" + definition.StableId, errors);
                if (envelope == null)
                {
                    continue;
                }

                ValidateParent(envelope, authorityCollisionRoot, "collision", definition.StableId, errors);
                ValidatePose(envelope, definition.LocalPose, "collision", definition.StableId, errors);
                BoxCollider collider = envelope.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    errors.Add("Collision " + definition.StableId + " is missing BoxCollider.");
                }
                else
                {
                    ValidateVector(collider.size, ToVector3(definition.Size), "collision size", definition.StableId, errors);
                }
            }
        }

        private static void ValidateVisualSlots(FunctionalRigVisualSlot[] definitions, Transform fallbackParent, Dictionary<string, Transform> joints, Dictionary<string, Transform> generated, List<string> errors)
        {
            foreach (FunctionalRigVisualSlot definition in definitions)
            {
                Transform slot = Get(generated, "visual-slot:" + definition.StableId, errors);
                Transform geometry = Get(generated, "geometry:" + definition.StableId, errors);
                if (slot == null || geometry == null)
                {
                    continue;
                }

                ValidateParent(slot, GetJointParent(definition.ParentStableId, joints, fallbackParent), "visual slot", definition.StableId, errors);
                ValidateVector(slot.localPosition, Vector3.zero, "visual slot local position", definition.StableId, errors);
                ValidateQuaternion(slot.localRotation, Quaternion.identity, "visual slot local rotation", definition.StableId, errors);
                ValidateVector(slot.localScale, Vector3.one, "visual slot local scale", definition.StableId, errors);
                ValidateParent(geometry, slot, "visual geometry", definition.StableId, errors);
                ValidateVector(geometry.localPosition, ToVector3(definition.ExpectedBounds.Center), "visual geometry center", definition.StableId, errors);
                ValidateQuaternion(geometry.localRotation, Quaternion.identity, "visual geometry local rotation", definition.StableId, errors);
                ValidateVector(geometry.localScale, ToVector3(definition.ExpectedBounds.Size), "visual geometry bounds", definition.StableId, errors);
                if (geometry.GetComponent<Collider>() != null)
                {
                    errors.Add("Visual geometry " + definition.StableId + " must not own gameplay collision.");
                }
            }
        }

        private static void ValidateNoUnexpectedGeneratedObjects(FunctionalRigContract contract, Dictionary<string, Transform> generated, List<string> errors)
        {
            var expected = new HashSet<string>(StringComparer.Ordinal);
            AddExpected(expected, "joint:", contract.Joints, definition => definition.StableId);
            AddExpected(expected, "anchor:", contract.Anchors, definition => definition.StableId);
            AddExpected(expected, "clearance:", contract.ClearanceVolumes, definition => definition.StableId);
            AddExpected(expected, "collision:", contract.CollisionEnvelopes, definition => definition.StableId);
            AddExpected(expected, "visual-slot:", contract.VisualSlots, definition => definition.StableId);
            AddExpected(expected, "geometry:", contract.VisualSlots, definition => definition.StableId);
            foreach (string stableId in generated.Keys)
            {
                if (!expected.Contains(stableId))
                {
                    errors.Add("Unexpected generated stable ID: " + stableId);
                }
            }
        }

        private static void ValidateVisibleSupportPaths(FunctionalRigPrototypeHierarchy hierarchy, List<string> errors)
        {
            foreach (Renderer renderer in hierarchy.GetComponentsInChildren<Renderer>(true))
            {
                Transform current = renderer.transform;
                bool reachesVisualSlot = false;
                while (current != null && current != hierarchy.transform)
                {
                    FunctionalRigPrototypeStableId identity = current.GetComponent<FunctionalRigPrototypeStableId>();
                    if (identity != null && identity.StableId.StartsWith("visual-slot:", StringComparison.Ordinal))
                    {
                        reachesVisualSlot = true;
                        break;
                    }

                    current = current.parent;
                }

                if (!reachesVisualSlot)
                {
                    errors.Add("Visible renderer lacks a declared visual-slot support path: " + renderer.name);
                }
            }
        }

        private static Transform Get(Dictionary<string, Transform> generated, string stableId, List<string> errors)
        {
            if (generated.TryGetValue(stableId, out Transform transform))
            {
                return transform;
            }

            errors.Add("Missing generated object: " + stableId);
            return null;
        }

        private static Transform GetJointParent(string stableId, Dictionary<string, Transform> joints, Transform fallbackParent)
        {
            if (string.IsNullOrEmpty(stableId))
            {
                return fallbackParent;
            }

            if (joints.TryGetValue(stableId, out Transform parent))
            {
                return parent;
            }

            return null;
        }

        private static void ValidateParent(Transform transform, Transform expectedParent, string kind, string stableId, List<string> errors)
        {
            if (expectedParent == null || transform.parent != expectedParent)
            {
                errors.Add(kind + " has invalid parent: " + stableId);
            }
        }

        private static void ValidatePose(Transform transform, ContractPose pose, string kind, string stableId, List<string> errors)
        {
            ValidateVector(transform.localPosition, ToVector3(pose.Position), kind + " local position", stableId, errors);
            ValidateQuaternion(transform.localRotation, Quaternion.Euler(ToVector3(pose.EulerDegrees)), kind + " local rotation", stableId, errors);
            ValidateVector(transform.localScale, Vector3.one, kind + " local scale", stableId, errors);
        }

        private static void ValidateVector(Vector3 actual, Vector3 expected, string property, string stableId, List<string> errors)
        {
            if ((actual - expected).sqrMagnitude > TransformTolerance * TransformTolerance)
            {
                errors.Add(property + " drifted: " + stableId);
            }
        }

        private static void ValidateQuaternion(Quaternion actual, Quaternion expected, string property, string stableId, List<string> errors)
        {
            if (Quaternion.Angle(actual, expected) > TransformTolerance)
            {
                errors.Add(property + " drifted: " + stableId);
            }
        }

        private static void AddExpected<T>(HashSet<string> expected, string prefix, IEnumerable<T> definitions, Func<T, string> getStableId)
        {
            foreach (T definition in definitions)
            {
                expected.Add(prefix + getStableId(definition));
            }
        }

        private static Vector3 ToVector3(ContractVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
    }
}
