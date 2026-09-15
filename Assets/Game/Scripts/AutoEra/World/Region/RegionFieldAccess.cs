using System;
using UnityEngine;

namespace AutoEra.World.Region
{
    [Serializable]
    public sealed class RegionFieldAccess
    {
        public float EnterDistance = 12f;
        public float ExitDistance = 16f;
        public float EnterHeight = 40f;
        public float ExitHeight = 48f;
        public float EnterViewportMargin = .05f;
        public float ExitViewportMargin = -.02f;

        public bool Evaluate(bool wasAccessible, float distance, float height, Vector3 viewport)
        {
            if (!RegionPlacement.IsFinite(distance) || !RegionPlacement.IsFinite(height) ||
                !RegionPlacement.IsFinite(viewport.x) || !RegionPlacement.IsFinite(viewport.y) || !RegionPlacement.IsFinite(viewport.z)) return false;
            if (!RegionPlacement.IsFinite(EnterDistance) || !RegionPlacement.IsFinite(ExitDistance) ||
                !RegionPlacement.IsFinite(EnterHeight) || !RegionPlacement.IsFinite(ExitHeight) ||
                !RegionPlacement.IsFinite(EnterViewportMargin) || !RegionPlacement.IsFinite(ExitViewportMargin)) return false;
            if (EnterDistance < 0 || ExitDistance < EnterDistance || EnterHeight < 0 || ExitHeight < EnterHeight ||
                ExitViewportMargin > EnterViewportMargin || EnterViewportMargin >= .5f) return false;
            float margin = wasAccessible ? ExitViewportMargin : EnterViewportMargin;
            return distance >= 0 && distance <= (wasAccessible ? ExitDistance : EnterDistance) &&
                height <= (wasAccessible ? ExitHeight : EnterHeight) && viewport.z > 0 &&
                viewport.x >= margin && viewport.x <= 1 - margin && viewport.y >= margin && viewport.y <= 1 - margin;
        }
    }
}
