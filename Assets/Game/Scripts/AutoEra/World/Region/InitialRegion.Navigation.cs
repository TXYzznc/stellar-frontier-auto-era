using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed partial class InitialRegion
    {
        public bool CanNavigate(PersistentId machine, Vector2 position, float yaw)
        {
            if (!TryGet(machine, out RegionObject body) || body.Machine == null || !body.Machine.Definition.CanMove ||
                !RegionPlacement.IsFinite(position) || !RegionPlacement.IsFinite(yaw) ||
                !RegionPlacement.Inside(Bounds, position, body.Size, yaw)) return false;
            foreach (RegionObject other in _objects.Values)
                if (other.Id != machine && other.BlocksNavigation &&
                    RegionPlacement.Overlaps(position, body.Size, yaw, other.Position, other.Size, other.Yaw)) return false;
            return true;
        }

        public bool TryUpdateMachinePose(PersistentId machine, Vector2 position, float yaw)
        {
            if (!CanNavigate(machine, position, yaw)) return false;
            var body = _objects[machine];
            body.Position = position; body.Yaw = yaw;
            return true;
        }
    }
}
