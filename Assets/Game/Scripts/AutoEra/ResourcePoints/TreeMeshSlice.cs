using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AutoEra.ResourcePoints
{
    /// <summary>Clips the approved static tree meshes at the authoritative horizontal fracture plane.</summary>
    public static class TreeMeshSlice
    {
        private struct Vertex
        {
            internal Vector3 Position, Normal;
            internal Vector4 Tangent;
            internal Vector2 UV, UV2;
            internal Color Color;
            internal static Vertex Lerp(Vertex a, Vertex b, float t) => new Vertex {
                Position = Vector3.LerpUnclamped(a.Position, b.Position, t), Normal = Vector3.LerpUnclamped(a.Normal, b.Normal, t).normalized,
                Tangent = Vector4.LerpUnclamped(a.Tangent, b.Tangent, t), UV = Vector2.LerpUnclamped(a.UV, b.UV, t),
                UV2 = Vector2.LerpUnclamped(a.UV2, b.UV2, t), Color = UnityEngine.Color.LerpUnclamped(a.Color, b.Color, t) };
        }
        public static Mesh Clip(Mesh source, Matrix4x4 toTree, float height, bool upper)
        {
            if (source == null || !source.isReadable || !ProductionRules.Finite(height)) throw new ArgumentException("Readable approved mesh and finite cut height required.");
            var positions = source.vertices; var normals = source.normals; var tangents = source.tangents;
            var uv = source.uv; var uv2 = source.uv2; var colors = source.colors;
            var vertices = new Vertex[positions.Length];
            for (int i = 0; i < vertices.Length; i++) vertices[i] = new Vertex {
                Position = positions[i], Normal = normals.Length == positions.Length ? normals[i] : Vector3.up,
                Tangent = tangents.Length == positions.Length ? tangents[i] : new Vector4(1,0,0,1),
                UV = uv.Length == positions.Length ? uv[i] : default, UV2 = uv2.Length == positions.Length ? uv2[i] : default,
                Color = colors.Length == positions.Length ? colors[i] : UnityEngine.Color.white };
            var output = new List<Vertex>(); var submeshes = new List<int>[source.subMeshCount]; var polygon = new Vertex[4];
            float sign = upper ? 1 : -1;
            for (int sub = 0; sub < submeshes.Length; sub++)
            {
                var indices = source.GetTriangles(sub); var triangles = submeshes[sub] = new List<int>();
                for (int t = 0; t < indices.Length; t += 3)
                {
                    int count = 0;
                    Vertex previous = vertices[indices[t + 2]];
                    float previousDistance = sign * (toTree.MultiplyPoint3x4(previous.Position).y - height);
                    for (int edge = 0; edge < 3; edge++)
                    {
                        Vertex current = vertices[indices[t + edge]];
                        float distance = sign * (toTree.MultiplyPoint3x4(current.Position).y - height);
                        if ((distance >= 0) != (previousDistance >= 0)) polygon[count++] = Vertex.Lerp(previous, current, previousDistance / (previousDistance - distance));
                        if (distance >= 0) polygon[count++] = current;
                        previous = current; previousDistance = distance;
                    }
                    if (count < 3) continue;
                    int first = output.Count; for (int i = 0; i < count; i++) output.Add(polygon[i]);
                    for (int i = 1; i < count - 1; i++) { triangles.Add(first); triangles.Add(first + i); triangles.Add(first + i + 1); }
                }
            }
            var result = new Mesh { name = source.name + (upper ? "_upper" : "_stump"), indexFormat = output.Count > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            var outPosition = new List<Vector3>(output.Count); var outNormal = new List<Vector3>(output.Count);
            var outTangent = new List<Vector4>(output.Count); var outUV = new List<Vector2>(output.Count); var outUV2 = new List<Vector2>(output.Count); var outColor = new List<Color>(output.Count);
            foreach (var vertex in output) { outPosition.Add(vertex.Position); outNormal.Add(vertex.Normal); outTangent.Add(vertex.Tangent); outUV.Add(vertex.UV); outUV2.Add(vertex.UV2); outColor.Add(vertex.Color); }
            result.SetVertices(outPosition); result.SetNormals(outNormal); result.SetTangents(outTangent); result.SetUVs(0, outUV); result.SetUVs(1, outUV2); result.SetColors(outColor);
            result.subMeshCount = submeshes.Length;
            for (int sub = 0; sub < submeshes.Length; sub++) result.SetTriangles(submeshes[sub], sub, false);
            result.RecalculateBounds(); return result;
        }
    }
}
