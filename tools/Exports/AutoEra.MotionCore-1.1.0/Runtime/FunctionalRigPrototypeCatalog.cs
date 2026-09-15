using System;
using AutoEra.Motion.Contracts;

namespace AutoEra.Motion
{
    /// <summary>Deterministic, contract-authoritative basic geometry for the first functional prototype families.</summary>
    public static class FunctionalRigPrototypeCatalog
    {
        public static readonly string[] AssetFamilyIds =
        {
            "wheeled_carrier", "four_wheel_module", "multi_joint_arm", "replaceable_effector", "sliding_door", "conveyor",
            "water_sprayer", "rotary_saw", "rotary_drill", "cargo_bay", "fixed_rotary_carrier"
        };

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
                case "water_sprayer": return WaterSprayer();
                case "rotary_saw": return RotarySaw();
                case "rotary_drill": return RotaryDrill();
                case "cargo_bay": return CargoBay();
                case "fixed_rotary_carrier": return FixedRotaryCarrier();
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
            FunctionalRigContract contract = Contract("multi_joint_arm", V(3.5f, 4f, 5.5f), new[]
            {
                J("base_yaw", "", V(0f, 0f, 0f), V(0f, 1f, 0f), "rotation", -3600f, 3600f),
                J("shoulder_pitch", "base_yaw", V(0f, 0.6f, 0f), V(1f, 0f, 0f), "rotation", -70f, 80f),
                J("elbow_pitch", "shoulder_pitch", V(0f, 0f, 2.1f), V(1f, 0f, 0f), "rotation", -120f, 120f),
                J("wrist_pitch", "elbow_pitch", V(0f, 0f, 1.8f), V(1f, 0f, 0f), "rotation", -100f, 100f),
                J("wrist_roll", "wrist_pitch", V(0f, 0f, 0.35f), V(0f, 0f, 1f), "rotation", -170f, 170f)
            }, new[] { A("workpoint_tool", "workpoint", "wrist_roll", V(0f, 0f, 0.7f)), A("socket_effector", "socket", "wrist_roll", V(0f, 0f, 0.45f)) },
            new[] { Volume("keepout_sweep", "keepout", "base_yaw", V(0f, 1f, 1.7f), V(4f, 3.5f, 5f)) },
            new[] { Volume("collision_base", "collision", "", V(0f, 0.45f, 0f), V(1.5f, 0.9f, 1.5f)) },
            new[] { S("base", "base_yaw", V(1.4f, 0.6f, 1.4f)), S("upper_arm", "shoulder_pitch", V(0.55f, 0.55f, 2.2f)), S("forearm", "elbow_pitch", V(0.42f, 0.42f, 1.9f)), S("wrist_link", "wrist_pitch", V(0.38f, 0.38f, 0.55f)), S("gripper", "wrist_roll", V(0.7f, 0.55f, 0.8f)) });
            contract.ContractVersion = "1.2.0";
            return contract;
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
                J("left_leaf", "frame", V(-1.15f, 0f, 0f), V(-1f, 0f, 0f), "translation", 0f, 1.2f),
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
                J("upper_idler_front", "frame", V(0f, 0.15f, 1.15f), V(1f, 0f, 0f), "rotation", -3600f, 3600f),
                J("upper_idler_rear", "frame", V(0f, 0.15f, -1.15f), V(1f, 0f, 0f), "rotation", -3600f, 3600f),
                J("lower_idler_front", "frame", V(0f, -0.28f, 1.15f), V(1f, 0f, 0f), "rotation", -3600f, 3600f),
                J("lower_idler_rear", "frame", V(0f, -0.28f, -1.15f), V(1f, 0f, 0f), "rotation", -3600f, 3600f)
            }, new[] { A("load", "socket", "frame", V(0f, 0.5f, -3f)), A("unload", "workpoint", "frame", V(0f, 0.5f, 3f)), A("block", "workpoint", "frame", V(0f, 0.85f, 0f)) },
            new[] { Volume("keepout_belt", "keepout", "frame", V(0f, 0.6f, 0f), V(3.5f, 1.6f, 6.5f)) },
            new[] { Volume("collision_frame", "collision", "", V(0f, 0.6f, 0f), V(3f, 1.2f, 6f)) },
            new[] { S("frame_visual", "frame", V(3f, 0.35f, 6f), V(0f, -0.3f, 0f)), S("belt_loop_visual", "frame", V(2.4f, 0.75f, 5.7f)), S("drive_roller_visual", "drive_roller", V(1.8f, 0.5f, 0.5f)), S("tail_roller_visual", "tail_roller", V(1.8f, 0.5f, 0.5f)), S("upper_idler_front_visual", "upper_idler_front", V(1.8f, 0.28f, 0.28f)), S("upper_idler_rear_visual", "upper_idler_rear", V(1.8f, 0.28f, 0.28f)), S("lower_idler_front_visual", "lower_idler_front", V(1.8f, 0.28f, 0.28f)), S("lower_idler_rear_visual", "lower_idler_rear", V(1.8f, 0.28f, 0.28f)) });
        }

        private static FunctionalRigContract WaterSprayer()
        {
            return Contract("water_sprayer", V(1.5f, 2.1f, 2.4f), new[]
            {
                J("mount_yaw", "", V(0f, 0.55f, 0f), V(0f, 1f, 0f), "rotation", -180f, 180f),
                J("nozzle_pitch", "mount_yaw", V(0f, 0.45f, 0.25f), V(1f, 0f, 0f), "rotation", -15f, 65f),
                J("flow_valve", "nozzle_pitch", V(0f, 0f, 0.42f), V(0f, 0f, 1f), "rotation", 0f, 90f)
            }, new[] { A("water_input", "socket", "mount_yaw", V(0f, -0.25f, -0.25f)), A("spray_origin", "workpoint", "flow_valve", V(0f, 0f, 0.42f)) },
            new[] { Volume("keepout_spray_head", "keepout", "mount_yaw", V(0f, 0.55f, 0.35f), V(1.6f, 1.4f, 2.6f)) },
            new[] { Volume("collision_sprayer", "collision", "", V(0f, 0.8f, 0f), V(1.5f, 2.1f, 2.4f)) },
            new[] { S("mount_body", "mount_yaw", V(1.25f, 0.9f, 1.2f)), S("nozzle", "nozzle_pitch", V(0.5f, 0.5f, 1.05f)), S("valve", "flow_valve", V(0.65f, 0.65f, 0.3f)) });
        }

        private static FunctionalRigContract RotarySaw()
        {
            return Contract("rotary_saw", V(2.2f, 2.6f, 2.4f), new[]
            {
                J("mount_yaw", "", V(0f, 0.65f, 0f), V(0f, 1f, 0f), "rotation", -90f, 90f),
                J("lift_rail", "mount_yaw", V(0f, 0.35f, 0.25f), V(0f, 1f, 0f), "translation", 0f, 0.75f),
                J("feed_slide", "lift_rail", V(0f, 0f, 0.3f), V(0f, 0f, 1f), "translation", 0f, 0.5f),
                J("saw_spindle", "feed_slide", V(0f, 0f, 0.55f), V(1f, 0f, 0f), "rotation", -3600f, 3600f)
            }, new[] { A("power_input", "socket", "mount_yaw", V(0f, -0.35f, -0.2f)), A("cut_point", "workpoint", "saw_spindle", V(0f, -0.55f, 0f)) },
            new[] { Volume("keepout_saw", "keepout", "mount_yaw", V(0f, 0.75f, 0.85f), V(2.3f, 2.4f, 2.5f)) },
            new[] { Volume("collision_saw", "collision", "", V(0f, 1.1f, 0f), V(2.2f, 2.6f, 2.4f)) },
            new[] { S("mount_body", "mount_yaw", V(1.6f, 1.1f, 1.3f)), S("rail", "lift_rail", V(0.7f, 1.5f, 0.7f)), S("feed_carriage", "feed_slide", V(0.9f, 0.8f, 0.8f)), S("saw_disc", "saw_spindle", V(0.15f, 1.3f, 1.3f)) });
        }

        private static FunctionalRigContract RotaryDrill()
        {
            return Contract("rotary_drill", V(2.1f, 3.1f, 2.1f), new[]
            {
                J("mount_yaw", "", V(0f, 0.6f, 0f), V(0f, 1f, 0f), "rotation", -180f, 180f),
                J("press_slide", "mount_yaw", V(0f, 0.95f, 0f), V(0f, 1f, 0f), "translation", -1.25f, 0f),
                J("drill_rotor", "press_slide", V(0f, -0.4f, 0.2f), V(0f, 1f, 0f), "rotation", -3600f, 3600f)
            }, new[] { A("power_input", "socket", "mount_yaw", V(0f, -0.3f, -0.35f)), A("drill_point", "workpoint", "drill_rotor", V(0f, -0.8f, 0.2f)) },
            new[] { Volume("keepout_drill", "keepout", "mount_yaw", V(0f, 0.6f, 0f), V(2.2f, 3.2f, 2.2f)) },
            new[] { Volume("collision_drill", "collision", "", V(0f, 1.2f, 0f), V(2.1f, 3.1f, 2.1f)) },
            new[] { S("mount_body", "mount_yaw", V(1.7f, 1.1f, 1.5f)), S("press_frame", "press_slide", V(0.85f, 1.5f, 0.85f)), S("drill_bit", "drill_rotor", V(0.55f, 1.6f, 0.55f)) });
        }

        private static FunctionalRigContract CargoBay()
        {
            return Contract("cargo_bay", V(3.2f, 2.4f, 3.6f), new[]
            {
                J("bay_frame", "", V(0f, 1.2f, 0f), V(0f, 1f, 0f), "rotation", 0f, 0f),
                J("left_door", "bay_frame", V(-0.72f, 0f, 1.45f), V(-1f, 0f, 0f), "translation", 0f, 0.75f),
                J("right_door", "bay_frame", V(0.72f, 0f, 1.45f), V(1f, 0f, 0f), "translation", 0f, 0.75f),
                J("transfer_tray", "bay_frame", V(0f, -0.42f, 0.15f), V(0f, 0f, 1f), "translation", 0f, 0.75f)
            }, new[] { A("cargo_access", "socket", "bay_frame", V(0f, 0f, 2.1f)), A("cargo_container", "workpoint", "transfer_tray", V(0f, 0.18f, 0f)) },
            new[] { Volume("keepout_cargo_bay", "keepout", "bay_frame", V(0f, 0f, 0f), V(3.4f, 2.6f, 3.8f)) },
            new[] { Volume("collision_cargo_bay", "collision", "", V(0f, 1.2f, 0f), V(3.2f, 2.4f, 3.6f)) },
            new[] { S("bay_floor", "bay_frame", V(2.7f, 0.2f, 2.8f), V(0f, -0.85f, -0.1f)), S("bay_left_wall", "bay_frame", V(0.18f, 1.8f, 2.8f), V(-1.26f, 0f, -0.1f)), S("bay_right_wall", "bay_frame", V(0.18f, 1.8f, 2.8f), V(1.26f, 0f, -0.1f)), S("bay_back_wall", "bay_frame", V(2.7f, 1.8f, 0.18f), V(0f, 0f, -1.41f)), S("left_door_panel", "left_door", V(1.2f, 1.8f, 0.18f)), S("right_door_panel", "right_door", V(1.2f, 1.8f, 0.18f)), S("transfer_tray_visual", "transfer_tray", V(2.35f, 0.16f, 2.35f)), S("cargo_volume", "transfer_tray", V(2.15f, 1.45f, 2.15f)) });
        }

        private static FunctionalRigContract FixedRotaryCarrier()
        {
            return Contract("fixed_rotary_carrier", V(2f, 1.45f, 2f), new[]
            {
                J("deployment_root", "", V(0f, 0.125f, 0f), V(0f, 1f, 0f), "rotation", 0f, 0f),
                J("yaw_pivot", "deployment_root", V(0f, 0.775f, 0f), V(0f, 1f, 0f), "rotation", -3600f, 3600f),
                J("effector_mount", "yaw_pivot", V(0f, 0.15f, 0.38f), V(0f, 1f, 0f), "rotation", 0f, 0f)
            }, new[]
            {
                A("effector_socket", "socket", "effector_mount", V(0f, 0f, 0.35f)),
                A("sensor_left", "socket", "deployment_root", V(-0.62f, 0.42f, -0.35f)),
                A("sensor_right", "socket", "deployment_root", V(0.62f, 0.42f, -0.35f)),
                A("core_socket", "socket", "deployment_root", V(0f, 0.46f, -0.52f)),
                A("container_port", "socket", "deployment_root", V(0f, 0.38f, 0.8f)),
                A("cargo_container", "workpoint", "deployment_root", V(0f, 0.45f, -0.1f))
            },
            new[] { Volume("keepout_base", "keepout", "deployment_root", V(0f, 0.5f, 0f), V(2.1f, 1.1f, 2.1f)) },
            new[] { Volume("collision_foundation", "collision", "", V(0f, 0.125f, 0f), V(2f, 0.25f, 2f)) },
            new[] { S("foundation", "deployment_root", V(2f, 0.25f, 2f)), S("support_column", "deployment_root", V(0.42f, 0.85f, 0.42f), V(0f, 0.43f, 0f)), S("turntable", "yaw_pivot", V(1.3f, 0.3f, 1.3f)), S("mount_head", "effector_mount", V(0.8f, 0.5f, 0.9f)) });
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
