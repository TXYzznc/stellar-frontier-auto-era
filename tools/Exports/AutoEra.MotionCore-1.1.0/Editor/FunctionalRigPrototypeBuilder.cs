using System;
using System.Collections.Generic;
using AutoEra.Motion;
using AutoEra.Motion.Contracts;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    /// <summary>
    /// Builds the declared functional structure into an existing hierarchy template.
    /// Stable IDs, rather than names or scene lookup, make repeated builds idempotent.
    /// </summary>
    public static class FunctionalRigPrototypeBuilder
    {
        private const string JointPrefix = "joint:";
        private const string AnchorPrefix = "anchor:";
        private const string ClearancePrefix = "clearance:";
        private const string CollisionPrefix = "collision:";
        private const string VisualSlotPrefix = "visual-slot:";
        private const string GeometryPrefix = "geometry:";

        public static void Build(FunctionalRigContract contract, FunctionalRigPrototypeHierarchy hierarchy)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (hierarchy == null)
            {
                throw new ArgumentNullException(nameof(hierarchy));
            }

            var contractErrors = new List<string>();
            if (!FunctionalRigContractValidator.TryValidate(contract, contractErrors))
            {
                throw new ArgumentException(string.Join(" ", contractErrors), nameof(contract));
            }

            if (!hierarchy.TryValidate(out string hierarchyError))
            {
                throw new ArgumentException(hierarchyError, nameof(hierarchy));
            }

            Dictionary<string, Transform> existing = CollectGeneratedObjects(hierarchy.transform);
            Dictionary<string, Transform> joints = BuildJoints(contract.Joints, hierarchy.VisualRoot, existing);
            BuildAnchors(contract.Anchors, joints, hierarchy.RigRoot, existing);
            BuildClearanceVolumes(contract.ClearanceVolumes, joints, hierarchy.RigRoot, existing);
            BuildCollisionEnvelopes(contract.CollisionEnvelopes, hierarchy.AuthorityCollisionRoot, existing);
            BuildVisualSlots(contract.VisualSlots, joints, hierarchy.VisualRoot, existing);
            RemoveStaleGeneratedObjects(existing, contract);
        }

        private static Dictionary<string, Transform> BuildJoints(FunctionalRigJoint[] definitions, Transform visualRoot, Dictionary<string, Transform> existing)
        {
            var joints = new Dictionary<string, Transform>(StringComparer.Ordinal);
            var remaining = new List<FunctionalRigJoint>(definitions);
            while (remaining.Count > 0)
            {
                bool builtOne = false;
                for (int index = remaining.Count - 1; index >= 0; index--)
                {
                    FunctionalRigJoint definition = remaining[index];
                    Transform parent;
                    if (string.IsNullOrEmpty(definition.ParentStableId))
                    {
                        parent = visualRoot;
                    }
                    else if (!joints.TryGetValue(definition.ParentStableId, out parent))
                    {
                        continue;
                    }

                    Transform joint = EnsureGeneratedChild(parent, JointPrefix + definition.StableId, "Joint_" + definition.StableId, existing);
                    ApplyPose(joint, definition.BindPose);
                    joints.Add(definition.StableId, joint);
                    remaining.RemoveAt(index);
                    builtOne = true;
                }

                if (!builtOne)
                {
                    throw new ArgumentException("Joint parent references contain a cycle or an unknown parent stable ID.", nameof(definitions));
                }
            }

            return joints;
        }

        private static void BuildAnchors(FunctionalRigAnchor[] definitions, Dictionary<string, Transform> joints, Transform fallbackParent, Dictionary<string, Transform> existing)
        {
            foreach (FunctionalRigAnchor definition in definitions)
            {
                Transform anchor = EnsureGeneratedChild(GetJointParent(definition.ParentStableId, joints, fallbackParent), AnchorPrefix + definition.StableId, "Anchor_" + definition.StableId, existing);
                ApplyPose(anchor, definition.LocalPose);
            }
        }

        private static void BuildClearanceVolumes(FunctionalRigVolume[] definitions, Dictionary<string, Transform> joints, Transform fallbackParent, Dictionary<string, Transform> existing)
        {
            foreach (FunctionalRigVolume definition in definitions)
            {
                Transform volume = EnsureGeneratedChild(GetJointParent(definition.ParentStableId, joints, fallbackParent), ClearancePrefix + definition.StableId, "Clearance_" + definition.StableId, existing);
                ApplyPose(volume, definition.LocalPose);
                volume.localScale = ToVector3(definition.Size);
            }
        }

        private static void BuildCollisionEnvelopes(FunctionalRigVolume[] definitions, Transform authorityCollisionRoot, Dictionary<string, Transform> existing)
        {
            foreach (FunctionalRigVolume definition in definitions)
            {
                Transform envelope = EnsureGeneratedChild(authorityCollisionRoot, CollisionPrefix + definition.StableId, "Collision_" + definition.StableId, existing);
                ApplyPose(envelope, definition.LocalPose);
                BoxCollider collider = envelope.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = envelope.gameObject.AddComponent<BoxCollider>();
                }

                collider.size = ToVector3(definition.Size);
            }
        }

        private static void BuildVisualSlots(FunctionalRigVisualSlot[] definitions, Dictionary<string, Transform> joints, Transform fallbackParent, Dictionary<string, Transform> existing)
        {
            foreach (FunctionalRigVisualSlot definition in definitions)
            {
                Transform slot = EnsureGeneratedChild(GetJointParent(definition.ParentStableId, joints, fallbackParent), VisualSlotPrefix + definition.StableId, "VisualSlot_" + definition.StableId, existing);
                slot.localPosition = Vector3.zero;
                slot.localRotation = Quaternion.identity;
                slot.localScale = Vector3.one;

                Transform geometry = EnsureGeneratedGeometry(slot, GeometryPrefix + definition.StableId, "Geometry_" + definition.StableId, existing);
                geometry.localPosition = ToVector3(definition.ExpectedBounds.Center);
                geometry.localRotation = Quaternion.identity;
                geometry.localScale = ToVector3(definition.ExpectedBounds.Size);
            }
        }

        private static Transform EnsureGeneratedGeometry(Transform parent, string stableId, string objectName, Dictionary<string, Transform> existing)
        {
            if (existing.TryGetValue(stableId, out Transform geometry))
            {
                geometry.SetParent(parent, false);
                geometry.name = objectName;
                return geometry;
            }

            GameObject geometryObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            geometryObject.name = objectName;
            geometryObject.transform.SetParent(parent, false);
            Collider visualCollider = geometryObject.GetComponent<Collider>();
            if (visualCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(visualCollider);
            }

            geometry = geometryObject.transform;
            AddStableId(geometry, stableId);
            existing.Add(stableId, geometry);
            return geometry;
        }

        private static Transform EnsureGeneratedChild(Transform parent, string stableId, string objectName, Dictionary<string, Transform> existing)
        {
            if (existing.TryGetValue(stableId, out Transform child))
            {
                child.SetParent(parent, false);
                child.name = objectName;
                return child;
            }

            var childObject = new GameObject(objectName);
            childObject.transform.SetParent(parent, false);
            AddStableId(childObject.transform, stableId);
            existing.Add(stableId, childObject.transform);
            return childObject.transform;
        }

        private static Dictionary<string, Transform> CollectGeneratedObjects(Transform root)
        {
            var result = new Dictionary<string, Transform>(StringComparer.Ordinal);
            foreach (FunctionalRigPrototypeStableId identity in root.GetComponentsInChildren<FunctionalRigPrototypeStableId>(true))
            {
                if (string.IsNullOrEmpty(identity.StableId))
                {
                    continue;
                }

                if (result.ContainsKey(identity.StableId))
                {
                    throw new ArgumentException("Prototype contains duplicate generated stable ID: " + identity.StableId, nameof(root));
                }

                result.Add(identity.StableId, identity.transform);
            }

            return result;
        }

        private static void RemoveStaleGeneratedObjects(Dictionary<string, Transform> existing, FunctionalRigContract contract)
        {
            var expected = new HashSet<string>(StringComparer.Ordinal);
            AddExpected(expected, JointPrefix, contract.Joints, definition => definition.StableId);
            AddExpected(expected, AnchorPrefix, contract.Anchors, definition => definition.StableId);
            AddExpected(expected, ClearancePrefix, contract.ClearanceVolumes, definition => definition.StableId);
            AddExpected(expected, CollisionPrefix, contract.CollisionEnvelopes, definition => definition.StableId);
            AddExpected(expected, VisualSlotPrefix, contract.VisualSlots, definition => definition.StableId);
            AddExpected(expected, GeometryPrefix, contract.VisualSlots, definition => definition.StableId);

            var staleIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, Transform> entry in existing)
            {
                if (!expected.Contains(entry.Key))
                {
                    staleIds.Add(entry.Key);
                }
            }

            foreach (KeyValuePair<string, Transform> entry in existing)
            {
                if (!staleIds.Contains(entry.Key))
                {
                    continue;
                }

                FunctionalRigPrototypeStableId parentIdentity = entry.Value.parent == null
                    ? null
                    : entry.Value.parent.GetComponent<FunctionalRigPrototypeStableId>();
                if (parentIdentity == null || !staleIds.Contains(parentIdentity.StableId))
                {
                    UnityEngine.Object.DestroyImmediate(entry.Value.gameObject);
                }
            }
        }

        private static void AddExpected<T>(HashSet<string> expected, string prefix, IEnumerable<T> definitions, Func<T, string> getStableId)
        {
            foreach (T definition in definitions)
            {
                expected.Add(prefix + getStableId(definition));
            }
        }

        private static Transform GetJointParent(string stableId, Dictionary<string, Transform> joints, Transform fallbackParent)
        {
            if (string.IsNullOrEmpty(stableId))
            {
                return fallbackParent;
            }

            if (!joints.TryGetValue(stableId, out Transform parent))
            {
                throw new ArgumentException("Contract references missing joint stable ID: " + stableId, nameof(stableId));
            }

            return parent;
        }

        private static void ApplyPose(Transform transform, ContractPose pose)
        {
            transform.localPosition = ToVector3(pose.Position);
            transform.localRotation = Quaternion.Euler(ToVector3(pose.EulerDegrees));
            transform.localScale = Vector3.one;
        }

        private static Vector3 ToVector3(ContractVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        private static void AddStableId(Transform transform, string stableId)
        {
            FunctionalRigPrototypeStableId identity = transform.gameObject.AddComponent<FunctionalRigPrototypeStableId>();
            identity.Configure(stableId);
        }
    }
}
