using System;
using AutoEra.Motion.Contracts;

namespace AutoEra.Motion
{
    /// <summary>Deterministic, contract-authoritative basic geometry for the first functional prototype families.</summary>
    public static class FunctionalRigPrototypeCatalog
    {
        public static readonly string[] AssetFamilyIds = { "wheeled_carrier", "four_wheel_module", "multi_joint_arm", "replaceable_effector", "sliding_door", "conveyor" };

        public static FunctionalRigContract Create(string assetFamilyId)
        {
            switch (assetFamilyId)
            {
                case "wheeled_carrier": return WheeledCarrier();
                case "four_wheel_module": return FourWheelModule();
                case "multi_joint_arm": return MultiJointArm();
                case "replaceable_effector": return ReplaceableEffector();
                case "sliding_door": return SlidingDoor();
                case "conveyor": return Conveyor();
                default: throw new ArgumentException("Unknown functional prototype family: " + assetFamilyId, nameof(assetFamilyId));
            }
        }

        private static FunctionalRigContract WheeledCarrier()
        {
            return Contract("wheeled_carrier", V(4f, 1.6f, 6f), new[]
            {
                J("chassis", "", V(0f, 0.8f, 0f), V(0f, 1f, 0f), "rotation", -12f, 12f),
                J("front_left_steer", "chassis", V(-1.65f, 0f, 2.05f), V(0f, 1f, 0f), "rotation", -35f, 35f), J("front_left_suspension", "front_left_steer", V(0f, 0f, 0f), V(0f, 1f, 0f), "translation", -0.35f, 0.15f), J("front_left_roll", "front_left_suspension", V(0f, 0f, 0f), V(1f, 0f, 0f), "rotation", -3600f, 3600f), J("front_left_contact", "front_left_roll", V(0f, -0.7f, 0f), V(0f, 1f, 0f), "translation", -0.15f, 0.15f),
                J("front_right_steer", "chassis", V(1.65f, 0f, 2.05f), V(0f, 1f, 0f), "rotation", -35f, 35f), J("front_right_suspension", "front_right_steer", V(0f, 0f, 0f), V(0f, 1f, 0f), "translation", -0.35f, 0.15f), J("front_right_roll", "front_right_suspension", V(0f, 0f, 0f), V(1f, 0f, 0f), "rotation", -3600f, 3600f), J("front_right_contact", "front_right_roll", V(0f, -0.7f, 0f), V(0f, 1f, 0f), "translation", -0.15f, 0.15f),
                J("rear_left_steer", "chassis", V(-1.65f, 0f, -2.05f), V(0f, 1f, 0f), "rotation", -35f, 35f), J("rear_left_suspension", "rear_left_steer", V(0f, 0f, 0f), V(0f, 1f, 0f), "translation", -0.35f, 0.15f), J("rear_left_roll", "rear_left_suspension", V(0f, 0f, 0f), V(1f, 0f, 0f), "rotation", -3600f, 3600f), J("rear_left_contact", "rear_left_roll", V(0f, -0.7f, 0f), V(0f, 1f, 0f), "translation", -0.15f, 0.15f),
                J("rear_right_steer", "chassis", V(1.65f, 0f, -2.05f), V(0f, 1f, 0f), "rotation", -35f, 35f), J("rear_right_suspension", "rear_right_steer", V(0f, 0f, 0f), V(0f, 1f, 0f), "translation", -0.35f, 0.15f), J("rear_right_roll", "rear_right_suspension", V(0f, 0f, 0f), V(1f, 0f, 0f), "rotation", -3600f, 3600f), J("rear_right_contact", "rear_right_roll", V(0f, -0.7f, 0f), V(0f, 1f, 0f), "translation", -0.15f, 0.15f)
            },
            new[] { A("driver_socket", "socket", "chassis", V(0f, 1f, 0.6f)), A("tow_workpoint", "workpoint", "chassis", V(0f, 0.5f, -3f)) },
            new[] { Volume("keepout_vehicle", "keepout", "chassis", V(0f, 0.8f, 0f), V(5f, 2.5f, 7f)) },
            new[] { Volume("collision_chassis", "collision", "", V(0f, 0.8f, 0f), V(4f, 1.6f, 6f)) },
            new[] { S("chassis_body", "chassis", V(4f, 1.1f, 5f)), S("front_left_tire", "front_left_roll", V(0.7f, 1.4f, 0.7f)), S("front_right_tire", "front_right_roll", V(0.7f, 1.4f, 0.7f)), S("rear_left_tire", "rear_left_roll", V(0.7f, 1.4f, 0.7f)), S("rear_right_tire", "rear_right_roll", V(0.7f, 1.4f, 0.7f)) });
        }

        private static FunctionalRigContract FourWheelModule()
        {
            return Contract("four_wheel_module", V(2f, 1.8f, 2f), new[]
            {
                J("mount", "", V(0f, 0.9f, 0f), V(0f, 1f, 0f), "rotation", -10f, 10f),
                J("steer", "mount", V(0f, 0f, 0f), V(0f, 1f, 0f), "rotation", -45f, 45f),
                J("suspension", "steer", V(0f, 0f, 0f), V(0f, 1f, 0f), "translation", -0.45f, 0.2f),
                J("roll", "suspension", V(0f, 0f, 0f), V(1f, 0f, 0f), "rotation", -3600f, 3600f),
                J("contact", "roll", V(0f, -0.75f, 0f), V(0f, 1f, 0f), "translation", -0.2f, 0.2f)
            }, new[] { A("ground_contact", "workpoint", "contact", V(0f, 0f, 0f)) },
            new[] { Volume("keepout_wheel", "keepout", "mount", V(0f, 0f, 0f), V(2.2f, 2f, 2.2f)) },
            new[] { Volume("collision_module", "collision", "", V(0f, 0.9f, 0f), V(2f, 1.8f, 2f)) },
            new[] { S("mount_frame", "mount", V(1.6f, 0.4f, 1.6f)), S("wheel", "roll", V(0.8f, 1.5f, 0.8f)) });
        }

        private static FunctionalRigContract MultiJointArm()
        {
            return Contract("multi_joint_arm", V(3f, 4f, 5f), new[]
            {
                J("yaw", "", V(0f, 0f, 0f), V(0f, 1f, 0f), "rotation", -160f, 160f),
                J("shoulder", "yaw", V(0f, 0.6f, 0f), V(1f, 0f, 0f), "rotation", -65f, 75f),
                J("extend", "shoulder", V(0f, 0f, 1.1f), V(0f, 0f, 1f), "translation", 0f, 2.2f),
                J("wrist", "extend", V(0f, 0f, 1.8f), V(0f, 0f, 1f), "rotation", -110f, 110f)
            }, new[] { A("workpoint_tool", "workpoint", "wrist", V(0f, 0f, 0.7f)), A("socket_effector", "socket", "wrist", V(0f, 0f, 0.45f)) },
            new[] { Volume("keepout_sweep", "keepout", "yaw", V(0f, 1f, 1.4f), V(3.5f, 3.5f, 4.5f)) },
            new[] { Volume("collision_base", "collision", "", V(0f, 0.45f, 0f), V(1.5f, 0.9f, 1.5f)) },
            new[] { S("base", "yaw", V(1.4f, 0.6f, 1.4f)), S("upper_arm", "shoulder", V(0.55f, 0.55f, 2.2f)), S("forearm", "extend", V(0.42f, 0.42f, 2.8f)), S("wrist_tool", "wrist", V(0.7f, 0.55f, 0.8f)) });
        }

        private static FunctionalRigContract ReplaceableEffector()
        {
            return Contract("replaceable_effector", V(2.2f, 1.8f, 2.2f), new[]
            {
                J("mount", "", V(0f, 0.9f, 0f), V(0f, 1f, 0f), "rotation", -30f, 30f),
                J("socket", "mount", V(0f, 0f, 0.45f), V(0f, 0f, 1f), "translation", 0f, 0.35f),
                J("lock", "socket", V(0f, 0f, 0.35f), V(0f, 0f, 1f), "rotation", 0f, 90f),
                J("safety_hold", "lock", V(0f, 0f, 0.3f), V(0f, 1f, 0f), "translation", -0.2f, 0f)
            }, new[] { A("effector_socket", "socket", "socket", V(0f, 0f, 0.3f)), A("safe_hold", "workpoint", "safety_hold", V(0f, -0.25f, 0f)) },
            new[] { Volume("keepout_effector", "keepout", "mount", V(0f, 0f, 0.7f), V(2f, 1.5f, 2f)) },
            new[] { Volume("collision_mount", "collision", "", V(0f, 0.9f, 0f), V(1.6f, 1.8f, 1.6f)) },
            new[] { S("mount_body", "mount", V(1.4f, 0.7f, 1.4f)), S("socket_ring", "socket", V(1.1f, 0.5f, 0.6f)), S("tool_head", "safety_hold", V(0.8f, 0.8f, 1f)) });
        }

        private static FunctionalRigContract SlidingDoor()
        {
            return Contract("sliding_door", V(5f, 3f, 0.6f), new[]
            {
                J("frame", "", V(0f, 1.5f, 0f), V(0f, 1f, 0f), "rotation", 0f, 0f),
                J("left_leaf", "frame", V(-1.15f, 0f, 0f), V(1f, 0f, 0f), "translation", -1.2f, 0f),
                J("right_leaf", "frame", V(1.15f, 0f, 0f), V(1f, 0f, 0f), "translation", 0f, 1.2f)
            }, new[] { A("passage_center", "workpoint", "frame", V(0f, 0f, 0f)) },
            new[] { Volume("safety_zone", "keepout", "frame", V(0f, 0f, 0.8f), V(4.5f, 3f, 2.2f)) },
            new[] { Volume("collision_frame", "collision", "", V(0f, 1.5f, 0f), V(5f, 3f, 0.6f)) },
            new[] { S("frame_top", "frame", V(5f, 0.35f, 0.5f), V(0f, 1.3f, 0f)), S("left_panel", "left_leaf", V(2f, 2.5f, 0.35f)), S("right_panel", "right_leaf", V(2f, 2.5f, 0.35f)) });
        }

        private static FunctionalRigContract Conveyor()
        {
            return Contract("conveyor", V(3f, 1.2f, 6f), new[]
            {
                J("frame", "", V(0f, 0.6f, 0f), V(0f, 1f, 0f), "rotation", -3f, 3f),
                J("drive_roller", "frame", V(0f, 0f, 2.5f), V(1f, 0f, 0f), "rotation", -3600f, 3600f),
                J("tail_roller", "frame", V(0f, 0f, -2.5f), V(1f, 0f, 0f), "rotation", -3600f, 3600f),
                J("belt", "frame", V(0f, 0.3f, 0f), V(0f, 0f, 1f), "translation", 0f, 1f)
            }, new[] { A("load", "socket", "frame", V(0f, 0.5f, -3f)), A("unload", "workpoint", "frame", V(0f, 0.5f, 3f)), A("block", "workpoint", "belt", V(0f, 0.4f, 0f)) },
            new[] { Volume("keepout_belt", "keepout", "frame", V(0f, 0.6f, 0f), V(3.5f, 1.6f, 6.5f)) },
            new[] { Volume("collision_frame", "collision", "", V(0f, 0.6f, 0f), V(3f, 1.2f, 6f)) },
            new[] { S("frame_visual", "frame", V(3f, 0.5f, 6f)), S("belt_surface", "belt", V(2.4f, 0.18f, 5.2f)), S("drive_roller_visual", "drive_roller", V(1.8f, 0.5f, 0.5f)), S("tail_roller_visual", "tail_roller", V(1.8f, 0.5f, 0.5f)) });
        }

        private static FunctionalRigContract Contract(string family, ContractVector3 bounds, FunctionalRigJoint[] joints, FunctionalRigAnchor[] anchors, FunctionalRigVolume[] clearance, FunctionalRigVolume[] collision, FunctionalRigVisualSlot[] slots)
        {
            return new FunctionalRigContract { ContractId = family + "_prototype", AssetFamilyId = family, ContractVersion = "1.1.0", OverallBounds = new ContractBounds { Size = bounds }, Compatibility = new FunctionalRigCompatibility { MinimumConsumerSchemaVersion = 1, MaximumConsumerSchemaVersion = 1, VisualReplacementCompatibilityId = family + "_visual_v1" }, Joints = joints, Anchors = anchors, ClearanceVolumes = clearance, CollisionEnvelopes = collision, VisualSlots = slots };
        }

        private static FunctionalRigJoint J(string id, string parent, ContractVector3 position, ContractVector3 axis, string channel, float minimum, float maximum) { return new FunctionalRigJoint { StableId = id, ParentStableId = parent, ObjectPath = "RigRoot/" + id, Channel = channel, LocalAxis = axis, MinimumValue = minimum, MaximumValue = maximum, BindPose = P(position), SafePose = P(position) }; }
        private static FunctionalRigAnchor A(string id, string kind, string parent, ContractVector3 position) { return new FunctionalRigAnchor { StableId = id, Kind = kind, ParentStableId = parent, LocalPose = P(position) }; }
        private static FunctionalRigVolume Volume(string id, string kind, string parent, ContractVector3 position, ContractVector3 size) { return new FunctionalRigVolume { StableId = id, Kind = kind, ParentStableId = parent, LocalPose = P(position), Size = size }; }
        private static FunctionalRigVisualSlot S(string id, string parent, ContractVector3 size) { return S(id, parent, size, V(0f, 0f, 0f)); }
        private static FunctionalRigVisualSlot S(string id, string parent, ContractVector3 size, ContractVector3 center) { return new FunctionalRigVisualSlot { StableId = id, ParentStableId = parent, ObjectPath = "RigRoot/" + parent + "/" + id, ExpectedBounds = new ContractBounds { Center = center, Size = size } }; }
        private static ContractPose P(ContractVector3 position) { return new ContractPose { Position = position }; }
        private static ContractVector3 V(float x, float y, float z) { return new ContractVector3 { X = x, Y = y, Z = z }; }
    }
}
