using System;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>Placement validation only. Construction/payment belongs to the caller.</summary>
    public sealed class RegionPlacementPreview : IDisposable
    {
        private readonly InitialRegion _region;
        private Action<Vector2, float> _confirmed;
        private bool _hasPosition;
        public RegionPlacementPreview(InitialRegion region, Vector2 size, Action<Vector2,float> confirmed)
        {
            if (region == null || !region.IsActive || !RegionPlacement.IsFinite(size) || size.x <= 0 || size.y <= 0)
                throw new ArgumentException("Invalid placement preview.");
            _region = region; Size = size; _confirmed = confirmed ?? throw new ArgumentNullException(nameof(confirmed));
            IsActive = true;
        }
        public Vector2 Size { get; }
        public Vector2 Position { get; private set; }
        public float Yaw { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsValid { get; private set; }
        public string Reason { get; private set; } = "请选择位置";
        public void Move(Vector2 position)
        {
            if (!IsActive) return;
            Position = RegionPlacement.SnapPosition(position); _hasPosition = true; Validate();
        }
        public void Rotate()
        {
            if (!IsActive) return;
            Yaw = RegionPlacement.SnapYaw(Yaw + 15); Validate();
        }
        private void Validate()
        {
            if (!_hasPosition) return;
            IsValid = _region.CanPlace(Position, Size, Yaw, PersistentId.Invalid, out string reason);
            Reason = reason;
        }
        public bool Confirm()
        {
            if (!IsActive) return false;
            Validate();
            if (!IsValid) return false;
            Action<Vector2,float> callback = _confirmed;
            Dispose();
            callback(Position, Yaw);
            return true;
        }
        public void Dispose() { IsActive = false; IsValid = false; _confirmed = null; }
    }
}
