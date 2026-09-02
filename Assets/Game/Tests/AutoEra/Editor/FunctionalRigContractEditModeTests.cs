using System.Collections.Generic;
using AutoEra.Motion.Contracts;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigContractEditModeTests
    {
        [Test]
        public void SerializeNormalized_SortsStableIdsAndKeepsFingerprintDeterministic()
        {
            FunctionalRigContract left = CreateValidContract();
            FunctionalRigContract right = CreateValidContract();
            right.Joints = new[] { right.Joints[1], right.Joints[0] };

            string leftJson = FunctionalRigContractJson.SerializeNormalized(left, out string leftFingerprint);
            string rightJson = FunctionalRigContractJson.SerializeNormalized(right, out string rightFingerprint);

            Assert.That(rightFingerprint, Is.EqualTo(leftFingerprint));
            Assert.That(rightJson, Is.EqualTo(leftJson));
            Assert.That(FunctionalRigContractJson.TryDeserialize(leftJson, out FunctionalRigContract parsed, out string error), Is.True, error);
            Assert.That(parsed.ContractId, Is.EqualTo(left.ContractId));
        }

        [Test]
        public void Validator_RejectsMissingFieldsDuplicateIdsIllegalPathsLimitsAndVersions()
        {
            FunctionalRigContract contract = CreateValidContract();
            contract.SchemaVersion = FunctionalRigContractSchema.CurrentVersion + 1;
            contract.Joints[1].StableId = contract.Joints[0].StableId;
            contract.Joints[0].ObjectPath = "../outside";
            contract.Joints[0].MinimumValue = 10f;
            contract.Joints[0].MaximumValue = -10f;

            var errors = new List<string>();

            Assert.That(FunctionalRigContractValidator.TryValidate(contract, errors), Is.False);
            Assert.That(errors, Has.Some.Contains("Unsupported schema version"));
            Assert.That(errors, Has.Some.Contains("Duplicate stableId"));
            Assert.That(errors, Has.Some.Contains("relative hierarchy path"));
            Assert.That(errors, Has.Some.Contains("minimumValue"));
        }

        [Test]
        public void Deserializer_RejectsMissingRequiredContractData()
        {
            const string InvalidJson = "{\"schemaVersion\":1,\"contractId\":\"machine\"}";

            Assert.That(FunctionalRigContractJson.TryDeserialize(InvalidJson, out _, out string error), Is.False);
            Assert.That(error, Does.Contain("assetFamilyId"));
        }

        private static FunctionalRigContract CreateValidContract()
        {
            return new FunctionalRigContract
            {
                ContractId = "prototype_carrier",
                AssetFamilyId = "wheeled_carrier",
                ContractVersion = "1.0.0",
                OverallBounds = new ContractBounds
                {
                    Center = new ContractVector3(),
                    Size = new ContractVector3 { X = 2f, Y = 1f, Z = 3f }
                },
                Compatibility = new FunctionalRigCompatibility
                {
                    MinimumConsumerSchemaVersion = 1,
                    MaximumConsumerSchemaVersion = 1,
                    VisualReplacementCompatibilityId = "carrier_v1"
                },
                Joints = new[]
                {
                    CreateJoint("arm_yaw", string.Empty),
                    CreateJoint("arm_pitch", "arm_yaw")
                },
                Anchors = new[]
                {
                    new FunctionalRigAnchor
                    {
                        StableId = "workpoint_main",
                        Kind = "workpoint",
                        ParentStableId = "arm_pitch"
                    }
                },
                ClearanceVolumes = new[]
                {
                    new FunctionalRigVolume
                    {
                        StableId = "keepout_arm",
                        Kind = "keepout",
                        ParentStableId = "arm_pitch",
                        Size = new ContractVector3 { X = 1f, Y = 1f, Z = 1f }
                    }
                },
                CollisionEnvelopes = new[]
                {
                    new FunctionalRigVolume
                    {
                        StableId = "collision_chassis",
                        Kind = "collision",
                        ParentStableId = string.Empty,
                        Size = new ContractVector3 { X = 2f, Y = 1f, Z = 3f }
                    }
                },
                VisualSlots = new[]
                {
                    new FunctionalRigVisualSlot
                    {
                        StableId = "slot_arm",
                        ParentStableId = "arm_pitch",
                        ObjectPath = "RigRoot/ArmPitch/VisualSlot",
                        ExpectedBounds = new ContractBounds
                        {
                            Center = new ContractVector3(),
                            Size = new ContractVector3 { X = 1f, Y = 1f, Z = 1f }
                        }
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
