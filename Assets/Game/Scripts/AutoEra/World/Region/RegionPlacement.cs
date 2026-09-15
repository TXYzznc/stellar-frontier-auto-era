using System;
using UnityEngine;

namespace AutoEra.World.Region
{
    public static class RegionPlacement
    {
        public static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public static bool IsFinite(Vector2 value) => IsFinite(value.x) && IsFinite(value.y);
        public static Vector2 SnapPosition(Vector2 value) => new Vector2(Snap(value.x, .5f), Snap(value.y, .5f));
        public static float SnapYaw(float value) => Mathf.Repeat(Snap(value, 15f), 360f);

        private static float Snap(float value, float step)
        {
            if (!IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            return (float)Math.Round(value / step, MidpointRounding.AwayFromZero) * step;
        }

        private static Vector2 Axis(float yaw)
        {
            float angle = yaw * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle));
        }

        private static float Radius(Vector2 size, Vector2 right, Vector2 axis)
        {
            Vector2 forward = new Vector2(-right.y, right.x);
            return (Mathf.Abs(Vector2.Dot(right, axis)) * size.x + Mathf.Abs(Vector2.Dot(forward, axis)) * size.y) * .5f;
        }

        public static bool Inside(Rect bounds, Vector2 position, Vector2 size, float yaw)
        {
            Vector2 right = Axis(yaw);
            float x = Radius(size, right, Vector2.right), y = Radius(size, right, Vector2.up);
            return position.x - x >= bounds.xMin && position.x + x <= bounds.xMax &&
                position.y - y >= bounds.yMin && position.y + y <= bounds.yMax;
        }

        public static bool Overlaps(Vector2 a, Vector2 aSize, float aYaw, Vector2 b, Vector2 bSize, float bYaw)
        {
            Vector2 ar = Axis(aYaw), br = Axis(bYaw), delta = b - a;
            return !Separated(delta, aSize, ar, bSize, br, ar) &&
                !Separated(delta, aSize, ar, bSize, br, new Vector2(-ar.y, ar.x)) &&
                !Separated(delta, aSize, ar, bSize, br, br) &&
                !Separated(delta, aSize, ar, bSize, br, new Vector2(-br.y, br.x));
        }

        private static bool Separated(Vector2 delta, Vector2 a, Vector2 ar, Vector2 b, Vector2 br, Vector2 axis)
            => Mathf.Abs(Vector2.Dot(delta, axis)) >= Radius(a, ar, axis) + Radius(b, br, axis) - .00001f;
    }
}
