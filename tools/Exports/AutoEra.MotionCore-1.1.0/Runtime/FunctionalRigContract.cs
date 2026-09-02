using System;

namespace AutoEra.Motion.Contracts
{
    /// <summary>
    /// Version constants shared by the serialized functional-rig contract and its validators.
    /// The contract is structural data only; it has no gameplay or Unity scene dependency.
    /// </summary>
    public static class FunctionalRigContractSchema
    {
        public const string SchemaId = "autoera/functional-rig-contract";
        public const int CurrentVersion = 1;
        public const string DefaultCoordinateConvention = "unity-left-handed-y-up-z-forward-meters";
    }

    /// <summary>
    /// Cross-project structural authority for a movable or key-assembly prototype.
    /// JSON field names are defined by FunctionalRigContract.schema.json; serialization is introduced separately.
    /// </summary>
    [Serializable]
    public sealed class FunctionalRigContract
    {
        public int SchemaVersion = FunctionalRigContractSchema.CurrentVersion;
        public string ContractId;
        public string AssetFamilyId;
        public string ContractVersion;
        public string ContentFingerprint;
        public ContractCoordinateConvention CoordinateConvention = ContractCoordinateConvention.CreateUnityDefault();
        public ContractBounds OverallBounds;
        public FunctionalRigJoint[] Joints = Array.Empty<FunctionalRigJoint>();
        public FunctionalRigAnchor[] Anchors = Array.Empty<FunctionalRigAnchor>();
        public FunctionalRigVolume[] ClearanceVolumes = Array.Empty<FunctionalRigVolume>();
        public FunctionalRigVolume[] CollisionEnvelopes = Array.Empty<FunctionalRigVolume>();
        public FunctionalRigVisualSlot[] VisualSlots = Array.Empty<FunctionalRigVisualSlot>();
        public FunctionalRigCompatibility Compatibility;
    }

    [Serializable]
    public sealed class ContractCoordinateConvention
    {
        public static ContractCoordinateConvention CreateUnityDefault()
        {
            return new ContractCoordinateConvention
            {
                Unit = "meter",
                UpAxis = "+Y",
                ForwardAxis = "+Z",
                Handedness = "left"
            };
        }

        public string Unit;
        public string UpAxis;
        public string ForwardAxis;
        public string Handedness;
    }

    [Serializable]
    public sealed class ContractBounds
    {
        public ContractVector3 Center;
        public ContractVector3 Size;
    }

    [Serializable]
    public sealed class FunctionalRigJoint
    {
        public string StableId;
        public string ParentStableId;
        public string ObjectPath;
        public string Channel;
        public ContractVector3 LocalAxis;
        public float MinimumValue;
        public float MaximumValue;
        public ContractPose BindPose;
        public ContractPose SafePose;
    }

    [Serializable]
    public sealed class FunctionalRigAnchor
    {
        public string StableId;
        public string Kind;
        public string ParentStableId;
        public ContractPose LocalPose;
    }

    [Serializable]
    public sealed class FunctionalRigVolume
    {
        public string StableId;
        public string Kind;
        public string ParentStableId;
        public ContractPose LocalPose;
        public ContractVector3 Size;
    }

    [Serializable]
    public sealed class FunctionalRigVisualSlot
    {
        public string StableId;
        public string ParentStableId;
        public string ObjectPath;
        public ContractBounds ExpectedBounds;
    }

    [Serializable]
    public sealed class FunctionalRigCompatibility
    {
        public int MinimumConsumerSchemaVersion;
        public int MaximumConsumerSchemaVersion;
        public string VisualReplacementCompatibilityId;
    }

    [Serializable]
    public struct ContractPose
    {
        public ContractVector3 Position;
        public ContractVector3 EulerDegrees;
    }

    [Serializable]
    public struct ContractVector3
    {
        public float X;
        public float Y;
        public float Z;
    }
}
