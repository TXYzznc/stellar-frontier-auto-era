using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Renders and advances one continuous conveyor belt mesh; it never advances logistics authority.</summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class ConveyorLoopRigPreview : MonoBehaviour, IMotionPreviewPresentation
    {
        private const int BaseSegmentCount = 64;
        private const int TreadCount = 36;
        private const int MarkerTreadInterval = 9;
        private const int MarkerTreadCount = TreadCount / MarkerTreadInterval;
        private const int BaseVerticesPerSegment = 4;
        private const int TreadVerticesPerSegment = 8;
        private const float BaseHalfThickness = 0.035f;
        private const float TreadHeight = 0.12f;
        private const float TreadLengthRatio = 0.88f;

        private readonly Transform[] _cargo = new Transform[8];
        private readonly Vector3[] _cargoBindPositions = new Vector3[8];
        private int _cargoCount;
        private Vector3[] _vertices;
        private int[] _beltTriangles;
        private int[] _motionMarkerTriangles;
        private Mesh _beltMesh;
        private Mesh _authoredMesh;
        private Mesh _editorPreviewMesh;
        private float _elapsed;

        private void Awake()
        {
            EnsureRuntimeMesh();
            CaptureCargo();
        }

        private void OnEnable()
        {
            if (_cargoCount == 0) CaptureCargo();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            ApplyVisualTime(_elapsed);
        }

        private void OnDisable()
        {
            for (int index = 0; index < _cargoCount; index++)
            {
                if (_cargo[index] != null) _cargo[index].localPosition = _cargoBindPositions[index];
            }
        }

        /// <summary>Called only by the catalog builder to make the same continuous belt visible in Edit Mode.</summary>
        public Mesh RebuildForEditor()
        {
            BuildMesh(GetComponent<MeshFilter>(), false);
            return _beltMesh;
        }

        public void ApplyPreviewTime(float elapsedSeconds)
        {
            if (UnityEngine.Application.isPlaying) return;
            MeshFilter filter = GetComponent<MeshFilter>();
            if (_editorPreviewMesh == null)
            {
                _authoredMesh = filter.sharedMesh;
                _editorPreviewMesh = Instantiate(_authoredMesh);
                _editorPreviewMesh.name = "连续闭环传送带_预览副本";
                _editorPreviewMesh.MarkDynamic();
                filter.sharedMesh = _editorPreviewMesh;
                _beltMesh = _editorPreviewMesh;
                PrepareVertexData();
            }

            ApplyVisualTime(Mathf.Max(0f, elapsedSeconds));
        }

        public void RestorePreviewBindPose()
        {
            if (UnityEngine.Application.isPlaying) return;
            MeshFilter filter = GetComponent<MeshFilter>();
            if (_editorPreviewMesh != null)
            {
                filter.sharedMesh = _authoredMesh;
                DestroyImmediate(_editorPreviewMesh);
                _editorPreviewMesh = null;
                _beltMesh = null;
            }

            RestoreCargoBindPositions();
        }

        private void EnsureRuntimeMesh()
        {
            BuildMesh(GetComponent<MeshFilter>(), true);
        }

        private void BuildMesh(MeshFilter filter, bool runtimeInstance)
        {
            PrepareVertexData();

            if (_beltMesh != null && UnityEngine.Application.isPlaying) Destroy(_beltMesh);
            _beltMesh = new Mesh { name = "连续闭环传送带" };
            _beltMesh.MarkDynamic();
            _beltMesh.SetVertices(_vertices);
            _beltMesh.subMeshCount = 2;
            _beltMesh.SetTriangles(_beltTriangles, 0);
            _beltMesh.SetTriangles(_motionMarkerTriangles, 1);
            _beltMesh.RecalculateNormals();
            _beltMesh.RecalculateBounds();
            if (runtimeInstance) filter.mesh = _beltMesh;
            else filter.sharedMesh = _beltMesh;
        }

        private void PrepareVertexData()
        {
            _vertices = new Vector3[BaseSegmentCount * BaseVerticesPerSegment + TreadCount * TreadVerticesPerSegment];
            _beltTriangles = new int[BaseSegmentCount * 24 + (TreadCount - MarkerTreadCount) * 36];
            _motionMarkerTriangles = new int[MarkerTreadCount * 36];
            BuildBaseVertices();
            UpdateTreadVertices(0f);
            BuildTriangles();
        }

        private void ApplyVisualTime(float elapsedSeconds)
        {
            if (_beltMesh == null || _vertices == null) return;
            if (_cargoCount == 0) CaptureCargo();
            UpdateTreadVertices(elapsedSeconds * ConveyorLoopPresentation.TreadSpeedMetersPerSecond / ConveyorLoopPresentation.LoopLength);
            _beltMesh.SetVertices(_vertices);
            _beltMesh.RecalculateBounds();

            float cargoProgress = elapsedSeconds * ConveyorLoopPresentation.TreadSpeedMetersPerSecond / ConveyorLoopPresentation.LoopLength;
            for (int index = 0; index < _cargoCount; index++)
            {
                _cargo[index].localPosition = ConveyorLoopPresentation.EvaluateCargoPosition(cargoProgress + (float)index / _cargoCount);
            }
        }

        private void RestoreCargoBindPositions()
        {
            for (int index = 0; index < _cargoCount; index++)
            {
                if (_cargo[index] != null) _cargo[index].localPosition = _cargoBindPositions[index];
            }
        }

        private void BuildBaseVertices()
        {
            float halfWidth = ConveyorLoopPresentation.HalfWidth - 0.04f;
            for (int index = 0; index < BaseSegmentCount; index++)
            {
                ConveyorLoopPresentation.EvaluateLoopPose((float)index / BaseSegmentCount, out Vector3 center, out Quaternion rotation);
                Vector3 right = rotation * Vector3.right;
                Vector3 up = rotation * Vector3.up;
                int offset = index * BaseVerticesPerSegment;
                _vertices[offset] = center - right * halfWidth + up * BaseHalfThickness;
                _vertices[offset + 1] = center + right * halfWidth + up * BaseHalfThickness;
                _vertices[offset + 2] = center + right * halfWidth - up * BaseHalfThickness;
                _vertices[offset + 3] = center - right * halfWidth - up * BaseHalfThickness;
            }
        }

        private void UpdateTreadVertices(float normalizedTravel)
        {
            float halfWidth = ConveyorLoopPresentation.HalfWidth - 0.03f;
            float halfLength = ConveyorLoopPresentation.LoopLength / TreadCount * TreadLengthRatio * 0.5f;
            int baseOffset = BaseSegmentCount * BaseVerticesPerSegment;
            for (int index = 0; index < TreadCount; index++)
            {
                ConveyorLoopPresentation.EvaluateLoopPose(normalizedTravel + (float)index / TreadCount, out Vector3 center, out Quaternion rotation);
                Vector3 right = rotation * Vector3.right;
                Vector3 up = rotation * Vector3.up;
                Vector3 forward = rotation * Vector3.forward;
                Vector3 lower = center + up * BaseHalfThickness;
                Vector3 upper = lower + up * TreadHeight;
                int offset = baseOffset + index * TreadVerticesPerSegment;
                _vertices[offset] = lower - right * halfWidth - forward * halfLength;
                _vertices[offset + 1] = lower + right * halfWidth - forward * halfLength;
                _vertices[offset + 2] = lower + right * halfWidth + forward * halfLength;
                _vertices[offset + 3] = lower - right * halfWidth + forward * halfLength;
                _vertices[offset + 4] = upper - right * halfWidth - forward * halfLength;
                _vertices[offset + 5] = upper + right * halfWidth - forward * halfLength;
                _vertices[offset + 6] = upper + right * halfWidth + forward * halfLength;
                _vertices[offset + 7] = upper - right * halfWidth + forward * halfLength;
            }
        }

        private void BuildTriangles()
        {
            int beltTriangleOffset = 0;
            for (int index = 0; index < BaseSegmentCount; index++)
            {
                int next = (index + 1) % BaseSegmentCount;
                AddQuad(_beltTriangles, ref beltTriangleOffset, index * 4, index * 4 + 1, next * 4 + 1, next * 4);
                AddQuad(_beltTriangles, ref beltTriangleOffset, index * 4 + 1, index * 4 + 2, next * 4 + 2, next * 4 + 1);
                AddQuad(_beltTriangles, ref beltTriangleOffset, index * 4 + 2, index * 4 + 3, next * 4 + 3, next * 4 + 2);
                AddQuad(_beltTriangles, ref beltTriangleOffset, index * 4 + 3, index * 4, next * 4, next * 4 + 3);
            }

            int treadBase = BaseSegmentCount * BaseVerticesPerSegment;
            int markerTriangleOffset = 0;
            for (int index = 0; index < TreadCount; index++)
            {
                int vertex = treadBase + index * TreadVerticesPerSegment;
                int[] triangles = index % MarkerTreadInterval == 0 ? _motionMarkerTriangles : _beltTriangles;
                if (triangles == _motionMarkerTriangles)
                {
                    AddTreadTriangles(triangles, ref markerTriangleOffset, vertex);
                }
                else
                {
                    AddTreadTriangles(triangles, ref beltTriangleOffset, vertex);
                }
            }
        }

        private static void AddTreadTriangles(int[] triangles, ref int offset, int vertex)
        {
            AddQuad(triangles, ref offset, vertex + 4, vertex + 5, vertex + 6, vertex + 7);
            AddQuad(triangles, ref offset, vertex, vertex + 3, vertex + 7, vertex + 4);
            AddQuad(triangles, ref offset, vertex + 1, vertex + 5, vertex + 6, vertex + 2);
            AddQuad(triangles, ref offset, vertex + 3, vertex + 2, vertex + 6, vertex + 7);
            AddQuad(triangles, ref offset, vertex, vertex + 4, vertex + 5, vertex + 1);
            AddQuad(triangles, ref offset, vertex, vertex + 1, vertex + 2, vertex + 3);
        }

        private static void AddQuad(int[] triangles, ref int offset, int a, int b, int c, int d)
        {
            triangles[offset] = a; triangles[offset + 1] = b; triangles[offset + 2] = c;
            triangles[offset + 3] = a; triangles[offset + 4] = c; triangles[offset + 5] = d;
            offset += 6;
        }

        private void CaptureCargo()
        {
            foreach (Transform candidate in GetComponentsInChildren<Transform>(true))
            {
                if (!candidate.name.StartsWith("DemoCargo_") || _cargoCount >= _cargo.Length) continue;
                _cargo[_cargoCount] = candidate;
                _cargoBindPositions[_cargoCount] = candidate.localPosition;
                _cargoCount++;
            }
        }
    }
}
