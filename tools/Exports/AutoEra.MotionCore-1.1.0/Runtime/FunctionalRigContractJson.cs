using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace AutoEra.Motion.Contracts
{
    /// <summary>
    /// Canonical JSON codec for FunctionalRigContract. It is used by editor tooling and tests,
    /// never by a per-frame presentation path.
    /// </summary>
    public static class FunctionalRigContractJson
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Culture = System.Globalization.CultureInfo.InvariantCulture,
            Formatting = Formatting.None
        };

        public static bool TryDeserialize(string json, out FunctionalRigContract contract, out string error)
        {
            contract = null;
            error = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Contract JSON is empty.";
                return false;
            }

            try
            {
                contract = JsonConvert.DeserializeObject<FunctionalRigContract>(json, SerializerSettings);
            }
            catch (JsonException exception)
            {
                error = exception.Message;
                return false;
            }

            var errors = new List<string>();
            if (!FunctionalRigContractValidator.TryValidate(contract, errors))
            {
                error = string.Join(" ", errors);
                contract = null;
                return false;
            }

            return true;
        }

        public static string SerializeNormalized(FunctionalRigContract contract, out string fingerprint)
        {
            var errors = new List<string>();
            if (!FunctionalRigContractValidator.TryValidate(contract, errors))
            {
                throw new ArgumentException(string.Join(" ", errors), nameof(contract));
            }

            JObject fingerprintPayload = ToCanonicalObject(contract, false);
            fingerprint = ComputeSha256(fingerprintPayload.ToString(Formatting.None));

            JObject serializedPayload = ToCanonicalObject(contract, true);
            serializedPayload["contentFingerprint"] = fingerprint;
            return Normalize(serializedPayload).ToString(Formatting.None);
        }

        private static JObject ToCanonicalObject(FunctionalRigContract contract, bool includeFingerprint)
        {
            JObject payload = JObject.FromObject(contract, JsonSerializer.Create(SerializerSettings));
            if (!includeFingerprint)
            {
                payload.Remove("contentFingerprint");
            }

            return (JObject)Normalize(payload);
        }

        private static JToken Normalize(JToken token)
        {
            if (token is JObject sourceObject)
            {
                var orderedObject = new JObject();
                foreach (JProperty property in sourceObject.Properties())
                {
                    orderedObject.Add(property.Name, Normalize(property.Value));
                }

                return orderedObject;
            }

            if (token is JArray sourceArray)
            {
                var normalizedItems = new List<JToken>(sourceArray.Count);
                foreach (JToken item in sourceArray)
                {
                    normalizedItems.Add(Normalize(item));
                }

                normalizedItems.Sort(CompareArrayItems);
                return new JArray(normalizedItems);
            }

            return token.DeepClone();
        }

        private static int CompareArrayItems(JToken left, JToken right)
        {
            string leftId = left["stableId"]?.Value<string>();
            string rightId = right["stableId"]?.Value<string>();
            if (leftId == null || rightId == null)
            {
                return string.CompareOrdinal(left.ToString(Formatting.None), right.ToString(Formatting.None));
            }

            return string.CompareOrdinal(leftId, rightId);
        }

        private static string ComputeSha256(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                var builder = new StringBuilder(hash.Length * 2);
                for (int index = 0; index < hash.Length; index++)
                {
                    builder.Append(hash[index].ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }
    }

    public static class FunctionalRigContractValidator
    {
        private static readonly Regex StableIdPattern = new Regex("^[a-z][a-z0-9_]*$", RegexOptions.CultureInvariant);

        public static bool TryValidate(FunctionalRigContract contract, List<string> errors)
        {
            if (errors == null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            errors.Clear();
            if (contract == null)
            {
                errors.Add("Contract is missing.");
                return false;
            }

            if (contract.SchemaVersion != FunctionalRigContractSchema.CurrentVersion)
            {
                errors.Add("Unsupported schema version.");
            }

            ValidateStableId(contract.ContractId, "contractId", errors);
            ValidateStableId(contract.AssetFamilyId, "assetFamilyId", errors);
            if (string.IsNullOrWhiteSpace(contract.ContractVersion))
            {
                errors.Add("contractVersion is required.");
            }

            ValidateCoordinateConvention(contract.CoordinateConvention, errors);
            ValidateBounds(contract.OverallBounds, "overallBounds", errors);
            ValidateCompatibility(contract.Compatibility, errors);

            var allStableIds = new HashSet<string>(StringComparer.Ordinal);
            var jointIds = new HashSet<string>(StringComparer.Ordinal);
            ValidateJoints(contract.Joints, allStableIds, jointIds, errors);
            ValidateAnchors(contract.Anchors, allStableIds, jointIds, errors);
            ValidateVolumes(contract.ClearanceVolumes, "clearanceVolumes", allStableIds, jointIds, errors);
            ValidateVolumes(contract.CollisionEnvelopes, "collisionEnvelopes", allStableIds, jointIds, errors);
            ValidateVisualSlots(contract.VisualSlots, allStableIds, jointIds, errors);
            return errors.Count == 0;
        }

        private static void ValidateCoordinateConvention(ContractCoordinateConvention convention, List<string> errors)
        {
            if (convention == null || convention.Unit != "meter" || convention.UpAxis != "+Y" || convention.ForwardAxis != "+Z" || convention.Handedness != "left")
            {
                errors.Add("coordinateConvention must be Unity left-handed meters with +Y up and +Z forward.");
            }
        }

        private static void ValidateCompatibility(FunctionalRigCompatibility compatibility, List<string> errors)
        {
            if (compatibility == null)
            {
                errors.Add("compatibility is required.");
                return;
            }

            if (compatibility.MinimumConsumerSchemaVersion < 1 || compatibility.MaximumConsumerSchemaVersion < compatibility.MinimumConsumerSchemaVersion)
            {
                errors.Add("compatibility has an invalid consumer schema range.");
            }

            ValidateStableId(compatibility.VisualReplacementCompatibilityId, "compatibility.visualReplacementCompatibilityId", errors);
        }

        private static void ValidateJoints(FunctionalRigJoint[] joints, HashSet<string> allStableIds, HashSet<string> jointIds, List<string> errors)
        {
            if (joints == null)
            {
                errors.Add("joints is required.");
                return;
            }

            foreach (FunctionalRigJoint joint in joints)
            {
                if (joint == null)
                {
                    errors.Add("joints contains a null value.");
                    continue;
                }

                ValidateAndRegisterStableId(joint.StableId, "joint", allStableIds, errors);
                if (!string.IsNullOrEmpty(joint.StableId))
                {
                    jointIds.Add(joint.StableId);
                }

                ValidateRelativePath(joint.ObjectPath, "joint objectPath", errors);
                ValidateStableId(joint.Channel, "joint channel", errors);
                if (IsZero(joint.LocalAxis))
                {
                    errors.Add("joint localAxis must not be zero.");
                }

                if (joint.MinimumValue > joint.MaximumValue)
                {
                    errors.Add("joint minimumValue must not exceed maximumValue.");
                }
            }
        }

        private static void ValidateAnchors(FunctionalRigAnchor[] anchors, HashSet<string> allStableIds, HashSet<string> jointIds, List<string> errors)
        {
            if (anchors == null)
            {
                errors.Add("anchors is required.");
                return;
            }

            foreach (FunctionalRigAnchor anchor in anchors)
            {
                if (anchor == null)
                {
                    errors.Add("anchors contains a null value.");
                    continue;
                }

                ValidateAndRegisterStableId(anchor.StableId, "anchor", allStableIds, errors);
                ValidateStableId(anchor.Kind, "anchor kind", errors);
                ValidateParent(anchor.ParentStableId, jointIds, "anchor", errors);
            }
        }

        private static void ValidateVolumes(FunctionalRigVolume[] volumes, string collectionName, HashSet<string> allStableIds, HashSet<string> jointIds, List<string> errors)
        {
            if (volumes == null)
            {
                errors.Add(collectionName + " is required.");
                return;
            }

            foreach (FunctionalRigVolume volume in volumes)
            {
                if (volume == null)
                {
                    errors.Add(collectionName + " contains a null value.");
                    continue;
                }

                ValidateAndRegisterStableId(volume.StableId, collectionName, allStableIds, errors);
                ValidateStableId(volume.Kind, collectionName + " kind", errors);
                ValidateParent(volume.ParentStableId, jointIds, collectionName, errors);
                if (volume.Size.X <= 0f || volume.Size.Y <= 0f || volume.Size.Z <= 0f)
                {
                    errors.Add(collectionName + " size must be positive.");
                }
            }
        }

        private static void ValidateVisualSlots(FunctionalRigVisualSlot[] visualSlots, HashSet<string> allStableIds, HashSet<string> jointIds, List<string> errors)
        {
            if (visualSlots == null)
            {
                errors.Add("visualSlots is required.");
                return;
            }

            foreach (FunctionalRigVisualSlot visualSlot in visualSlots)
            {
                if (visualSlot == null)
                {
                    errors.Add("visualSlots contains a null value.");
                    continue;
                }

                ValidateAndRegisterStableId(visualSlot.StableId, "visualSlot", allStableIds, errors);
                ValidateParent(visualSlot.ParentStableId, jointIds, "visualSlot", errors);
                ValidateRelativePath(visualSlot.ObjectPath, "visualSlot objectPath", errors);
                ValidateBounds(visualSlot.ExpectedBounds, "visualSlot expectedBounds", errors);
            }
        }

        private static void ValidateBounds(ContractBounds bounds, string name, List<string> errors)
        {
            if (bounds == null || bounds.Size.X <= 0f || bounds.Size.Y <= 0f || bounds.Size.Z <= 0f)
            {
                errors.Add(name + " must have positive size.");
            }
        }

        private static void ValidateParent(string parentStableId, HashSet<string> jointIds, string kind, List<string> errors)
        {
            if (!string.IsNullOrEmpty(parentStableId) && !jointIds.Contains(parentStableId))
            {
                errors.Add(kind + " parentStableId must reference a declared joint.");
            }
        }

        private static void ValidateRelativePath(string path, string name, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(path) || path.StartsWith("/", StringComparison.Ordinal) || path.StartsWith("\\", StringComparison.Ordinal) || path.IndexOf("..", StringComparison.Ordinal) >= 0)
            {
                errors.Add(name + " must be a non-empty relative hierarchy path.");
            }
        }

        private static void ValidateAndRegisterStableId(string value, string name, HashSet<string> allStableIds, List<string> errors)
        {
            ValidateStableId(value, name, errors);
            if (!string.IsNullOrEmpty(value) && !allStableIds.Add(value))
            {
                errors.Add("Duplicate stableId: " + value + ".");
            }
        }

        private static void ValidateStableId(string value, string name, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value) || !StableIdPattern.IsMatch(value))
            {
                errors.Add(name + " must be a lowercase stable identifier.");
            }
        }

        private static bool IsZero(ContractVector3 value)
        {
            return value.X == 0f && value.Y == 0f && value.Z == 0f;
        }
    }
}
