using System;

namespace AutoEra.World.Identity
{
    /// <summary>
    /// Serializable identity-based reference. It intentionally does not retain a display
    /// name, prefab identity, or object instance, so a missing target cannot be rebound
    /// implicitly to a similar object.
    /// </summary>
    [Serializable]
    public readonly struct PersistentObjectReference : IEquatable<PersistentObjectReference>
    {
        public PersistentObjectReference(PersistentId id, PersistentObjectKind expectedKind)
        {
            Id = id;
            ExpectedKind = expectedKind;
        }

        public PersistentId Id { get; }

        public PersistentObjectKind ExpectedKind { get; }

        public bool IsValid => Id.IsValid && ExpectedKind != PersistentObjectKind.None;

        public PersistentRegistryResult TryResolve(PersistentObjectRegistry registry, out object instance)
        {
            instance = null;
            if (registry == null)
            {
                return PersistentRegistryResult.Missing;
            }

            return registry.TryResolve(Id, ExpectedKind, out instance);
        }

        public bool Equals(PersistentObjectReference other)
        {
            return Id == other.Id && ExpectedKind == other.ExpectedKind;
        }

        public override bool Equals(object obj)
        {
            return obj is PersistentObjectReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ (int)ExpectedKind;
            }
        }

        public static bool operator ==(PersistentObjectReference left, PersistentObjectReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PersistentObjectReference left, PersistentObjectReference right)
        {
            return !left.Equals(right);
        }
    }
}
