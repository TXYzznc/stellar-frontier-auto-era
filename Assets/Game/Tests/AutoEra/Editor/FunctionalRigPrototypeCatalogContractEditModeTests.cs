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
            Assert.That(jointCounts[2], Is.EqualTo(4));
            Assert.That(jointCounts[3], Is.EqualTo(4));
            Assert.That(jointCounts[4], Is.EqualTo(3));
            Assert.That(jointCounts[5], Is.EqualTo(4));
            Assert.That(visualSlotCounts, Is.Not.All.EqualTo(1));
        }

        [Test]
        public void Catalog_DeclaresRequiredFunctionalAnchorsAndSafetyVolumes()
        {
            FunctionalRigContract arm = FunctionalRigPrototypeCatalog.Create("multi_joint_arm");
            FunctionalRigContract effector = FunctionalRigPrototypeCatalog.Create("replaceable_effector");
            FunctionalRigContract door = FunctionalRigPrototypeCatalog.Create("sliding_door");
            FunctionalRigContract conveyor = FunctionalRigPrototypeCatalog.Create("conveyor");

            Assert.That(HasAnchor(arm, "workpoint_tool"), Is.True);
            Assert.That(HasAnchor(arm, "socket_effector"), Is.True);
            Assert.That(HasAnchor(effector, "effector_socket"), Is.True);
            Assert.That(HasAnchor(effector, "safe_hold"), Is.True);
            Assert.That(HasVolume(door, "safety_zone"), Is.True);
            Assert.That(HasAnchor(conveyor, "load"), Is.True);
            Assert.That(HasAnchor(conveyor, "unload"), Is.True);
            Assert.That(HasAnchor(conveyor, "block"), Is.True);
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
    }
}
