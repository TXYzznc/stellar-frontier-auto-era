using System;
using AutoEra.Motion.Contracts;

namespace AutoEra.Motion
{
    public static class FunctionalRigPrototypeCatalog
    {
        public static readonly string[] AssetFamilyIds = { "wheeled_carrier", "four_wheel_module", "multi_joint_arm", "replaceable_effector", "sliding_door", "conveyor" };

        public static FunctionalRigContract Create(string assetFamilyId)
        {
            if (Array.IndexOf(AssetFamilyIds, assetFamilyId) < 0) throw new ArgumentException("Unknown functional prototype family: " + assetFamilyId, nameof(assetFamilyId));
            return new FunctionalRigContract
            {
                ContractId = assetFamilyId + "_prototype",
                AssetFamilyId = assetFamilyId,
                ContractVersion = "1.0.0",
                OverallBounds = new ContractBounds { Size = new ContractVector3 { X = 2f, Y = 1f, Z = 3f } },
                Compatibility = new FunctionalRigCompatibility { MinimumConsumerSchemaVersion = 1, MaximumConsumerSchemaVersion = 1, VisualReplacementCompatibilityId = assetFamilyId + "_visual_v1" },
                Joints = new[] { CreateJoint("primary", string.Empty) },
                Anchors = new[] { new FunctionalRigAnchor { StableId = "workpoint", Kind = "workpoint", ParentStableId = "primary" } },
                ClearanceVolumes = new[] { new FunctionalRigVolume { StableId = "keepout", Kind = "keepout", ParentStableId = "primary", Size = new ContractVector3 { X = 1f, Y = 1f, Z = 1f } } },
                CollisionEnvelopes = new[] { new FunctionalRigVolume { StableId = "collision", Kind = "collision", Size = new ContractVector3 { X = 2f, Y = 1f, Z = 3f } } },
                VisualSlots = new[] { new FunctionalRigVisualSlot { StableId = "body", ParentStableId = "primary", ObjectPath = "RigRoot/primary/VisualSlot", ExpectedBounds = new ContractBounds { Size = new ContractVector3 { X = 2f, Y = 1f, Z = 3f } } } }
            };
        }

        private static FunctionalRigJoint CreateJoint(string stableId, string parentStableId)
        {
            return new FunctionalRigJoint { StableId = stableId, ParentStableId = parentStableId, ObjectPath = "RigRoot/" + stableId, Channel = "rotation", LocalAxis = new ContractVector3 { Y = 1f }, MinimumValue = -90f, MaximumValue = 90f };
        }
    }
}
