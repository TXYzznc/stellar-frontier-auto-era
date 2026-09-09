using AutoEra.Motion;
using AutoEra.Motion.Contracts;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigPrototypeCatalogContractEditModeTests
    {
        [Test]
        public void Catalog_DeclaresDistinctFunctionalHierarchiesForEveryFamily()
        {
            int[] jointCounts = new int[FunctionalRigPrototypeCatalog.AssetFamilyIds.Length];
            int[] visualSlotCounts = new int[FunctionalRigPrototypeCatalog.AssetFamilyIds.Length];
            for (int index = 0; index < FunctionalRigPrototypeCatalog.AssetFamilyIds.Length; index++)
            {
                FunctionalRigContract contract = FunctionalRigPrototypeCatalog.Create(FunctionalRigPrototypeCatalog.AssetFamilyIds[index]);
                var errors = new System.Collections.Generic.List<string>();
                Assert.That(FunctionalRigContractValidator.TryValidate(contract, errors), Is.True, string.Join(" ", errors));
                jointCounts[index] = contract.Joints.Length;
                visualSlotCounts[index] = contract.VisualSlots.Length;
            }

            Assert.That(jointCounts[0], Is.EqualTo(17), "carrier must declare a chassis and four independent steer/suspension/roll/contact chains.");
            Assert.That(jointCounts[1], Is.EqualTo(5));
            Assert.That(jointCounts[2], Is.EqualTo(5));
            Assert.That(jointCounts[3], Is.EqualTo(4));
            Assert.That(jointCounts[4], Is.EqualTo(3));
            Assert.That(jointCounts[5], Is.EqualTo(7), "conveyor requires end rollers and four support rollers around one closed belt loop.");
            Assert.That(jointCounts[6], Is.EqualTo(3), "water sprayer requires yaw, pitch and valve joints.");
            Assert.That(jointCounts[7], Is.EqualTo(4), "rotary saw requires yaw, lift, feed and spindle joints.");
            Assert.That(jointCounts[8], Is.EqualTo(3), "rotary drill requires yaw, press and rotor joints.");
            Assert.That(jointCounts[9], Is.EqualTo(4), "cargo bay requires a frame, two door panels and an extractable transfer tray.");
            Assert.That(jointCounts[10], Is.EqualTo(3), "fixed rotary carrier requires deployment, yaw and effector mount structure.");
            Assert.That(visualSlotCounts, Is.Not.All.EqualTo(1));
        }

        [Test]
        public void Catalog_DeclaresRequiredFunctionalAnchorsAndSafetyVolumes()
        {
            FunctionalRigContract arm = FunctionalRigPrototypeCatalog.Create("multi_joint_arm");
            FunctionalRigContract effector = FunctionalRigPrototypeCatalog.Create("replaceable_effector");
            FunctionalRigContract door = FunctionalRigPrototypeCatalog.Create("sliding_door");
            FunctionalRigContract conveyor = FunctionalRigPrototypeCatalog.Create("conveyor");
            FunctionalRigContract waterSprayer = FunctionalRigPrototypeCatalog.Create("water_sprayer");
            FunctionalRigContract rotarySaw = FunctionalRigPrototypeCatalog.Create("rotary_saw");
            FunctionalRigContract rotaryDrill = FunctionalRigPrototypeCatalog.Create("rotary_drill");
            FunctionalRigContract cargoBay = FunctionalRigPrototypeCatalog.Create("cargo_bay");
            FunctionalRigContract fixedRotaryCarrier = FunctionalRigPrototypeCatalog.Create("fixed_rotary_carrier");

            Assert.That(HasAnchor(arm, "workpoint_tool"), Is.True);
            Assert.That(arm.ContractVersion, Is.EqualTo("1.2.0"));
            Assert.That(HasAnchor(arm, "socket_effector"), Is.True);
            Assert.That(HasJoint(arm, "base_yaw", "", -3600f, 3600f), Is.True);
            Assert.That(HasJoint(arm, "shoulder_pitch", "base_yaw", -70f, 80f), Is.True);
            Assert.That(HasJoint(arm, "elbow_pitch", "shoulder_pitch", -120f, 120f), Is.True);
            Assert.That(HasJoint(arm, "wrist_pitch", "elbow_pitch", -100f, 100f), Is.True);
            Assert.That(HasJoint(arm, "wrist_roll", "wrist_pitch", -170f, 170f), Is.True);
            Assert.That(HasAnchor(effector, "effector_socket"), Is.True);
            Assert.That(HasAnchor(effector, "safe_hold"), Is.True);
            Assert.That(HasVolume(door, "safety_zone"), Is.True);
            Assert.That(HasJoint(door, "left_leaf", "frame", 0f, 1.2f), Is.True);
            Assert.That(HasJoint(door, "right_leaf", "frame", 0f, 1.2f), Is.True);
            Assert.That(HasAnchor(conveyor, "load"), Is.True);
            Assert.That(HasAnchor(conveyor, "unload"), Is.True);
            Assert.That(HasAnchor(conveyor, "block"), Is.True);
            Assert.That(HasVisualSlot(conveyor, "belt_loop_visual"), Is.True);
            Assert.That(HasAnchor(waterSprayer, "water_input"), Is.True);
            Assert.That(HasAnchor(waterSprayer, "spray_origin"), Is.True);
            Assert.That(HasJoint(waterSprayer, "nozzle_pitch", "mount_yaw", -15f, 65f), Is.True);
            Assert.That(HasAnchor(rotarySaw, "cut_point"), Is.True);
            Assert.That(HasJoint(rotarySaw, "saw_spindle", "feed_slide", -3600f, 3600f), Is.True);
            Assert.That(HasAnchor(rotaryDrill, "drill_point"), Is.True);
            Assert.That(HasJoint(rotaryDrill, "press_slide", "mount_yaw", -1.25f, 0f), Is.True);
            Assert.That(HasAnchor(cargoBay, "cargo_access"), Is.True);
            Assert.That(HasAnchor(cargoBay, "cargo_container"), Is.True);
            Assert.That(HasJoint(cargoBay, "transfer_tray", "bay_frame", 0f, 0.75f), Is.True);
            Assert.That(HasJoint(fixedRotaryCarrier, "yaw_pivot", "deployment_root", -3600f, 3600f), Is.True);
            Assert.That(HasAnchor(fixedRotaryCarrier, "effector_socket"), Is.True);
            Assert.That(HasAnchor(fixedRotaryCarrier, "sensor_left"), Is.True);
            Assert.That(HasAnchor(fixedRotaryCarrier, "sensor_right"), Is.True);
            Assert.That(HasAnchor(fixedRotaryCarrier, "core_socket"), Is.True);
            Assert.That(HasAnchor(fixedRotaryCarrier, "container_port"), Is.True);
            Assert.That(HasVisualSlot(fixedRotaryCarrier, "support_column"), Is.True);
        }

        private static bool HasAnchor(FunctionalRigContract contract, string stableId)
        {
            foreach (FunctionalRigAnchor anchor in contract.Anchors) if (anchor.StableId == stableId) return true;
            return false;
        }

        private static bool HasVolume(FunctionalRigContract contract, string stableId)
        {
            foreach (FunctionalRigVolume volume in contract.ClearanceVolumes) if (volume.StableId == stableId) return true;
            return false;
        }

        private static bool HasVisualSlot(FunctionalRigContract contract, string stableId)
        {
            foreach (FunctionalRigVisualSlot slot in contract.VisualSlots) if (slot.StableId == stableId) return true;
            return false;
        }

        private static bool HasJoint(FunctionalRigContract contract, string stableId, string parentStableId, float minimum, float maximum)
        {
            foreach (FunctionalRigJoint joint in contract.Joints)
            {
                if (joint.StableId == stableId && joint.ParentStableId == parentStableId && joint.MinimumValue == minimum && joint.MaximumValue == maximum) return true;
            }

            return false;
        }
    }
}
