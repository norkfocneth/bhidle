using System.Collections.Generic;
using UnityEngine;

namespace TERRAGRAV.Trail
{
    /// <summary>
    /// Procedural 2.5D Ribbon Mesh Generator for player and bot trails.
    /// Rebuilds dynamic ribbon geometry without creating per-point GameObjects or heap allocations.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TrailRendererController : MonoBehaviour
    {
        [Header("Ribbon Visual Settings")]
        [Tooltip("Width of the 2.5D ribbon trail in world units.")]
        [SerializeField] private float _trailWidth = 0.6f;

        [Tooltip("Elevation of the ribbon above the arena floor.")]
        [SerializeField] private float _elevationY = 0.12f;

        [Tooltip("Material used for the trail ribbon mesh.")]
        [SerializeField] private Material _trailMaterial;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;
        private Color _playerColor = Color.cyan;

        // Reusable buffers to eliminate garbage collection during 60 FPS update loops
        private readonly List<Vector3> _vertices = new List<Vector3>(1024);
        private readonly List<int> _triangles = new List<int>(3072);
        private readonly List<Color> _colors = new List<Color>(1024);
        private readonly List<Vector2> _uvs = new List<Vector2>(1024);
        private readonly List<Vector3> _normals = new List<Vector3>(1024);

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _mesh = new Mesh
            {
                name = "ProceduralTrailRibbon_Mesh"
            };
            _mesh.MarkDynamic();
            _meshFilter.sharedMesh = _mesh;

            if (_trailMaterial != null)
            {
                _meshRenderer.sharedMaterial = _trailMaterial;
            }
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        public void SetColor(Color color)
        {
            _playerColor = color;
        }

        public void SetMaterial(Material mat)
        {
            _trailMaterial = mat;
            if (_meshRenderer != null)
            {
                _meshRenderer.sharedMaterial = mat;
            }
        }

        public void SetWidth(float width)
        {
            _trailWidth = Mathf.Max(0.1f, width);
        }

        /// <summary>
        /// Reconstructs the 2.5D ribbon mesh using the sampled trail points and current head position.
        /// </summary>
        public void UpdateMesh(IReadOnlyList<TrailPoint> points, Vector3 currentHeadPos, bool isOutside)
        {
            _vertices.Clear();
            _triangles.Clear();
            _colors.Clear();
            _uvs.Clear();
            _normals.Clear();

            int pointCount = points != null ? points.Count : 0;
            if (pointCount == 0 && !isOutside)
            {
                _mesh.Clear();
                return;
            }

            // Construct list of world points including active head
            int totalNodes = pointCount + (isOutside ? 1 : 0);
            if (totalNodes < 2)
            {
                _mesh.Clear();
                return;
            }

            float halfWidth = _trailWidth * 0.5f;

            for (int i = 0; i < totalNodes; i++)
            {
                Vector3 currentPos = (i < pointCount) ? points[i].position : currentHeadPos;
                currentPos.y = _elevationY;

                // Calculate forward and perpendicular tangent vectors
                Vector3 forward;
                if (i < totalNodes - 1)
                {
                    Vector3 nextPos = (i + 1 < pointCount) ? points[i + 1].position : currentHeadPos;
                    forward = (nextPos - currentPos).normalized;
                }
                else
                {
                    Vector3 prevPos = (i - 1 < pointCount) ? points[i - 1].position : currentHeadPos;
                    forward = (currentPos - prevPos).normalized;
                }

                if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;

                // Perpendicular in X-Z horizontal plane
                Vector3 right = new Vector3(-forward.z, 0f, forward.x).normalized;

                // Left and Right vertices for ribbon edge
                Vector3 leftVert = currentPos - (right * halfWidth);
                Vector3 rightVert = currentPos + (right * halfWidth);

                _vertices.Add(leftVert);
                _vertices.Add(rightVert);

                float u = (float)i / (totalNodes - 1);
                _uvs.Add(new Vector2(0f, u));
                _uvs.Add(new Vector2(1f, u));

                _colors.Add(_playerColor);
                _colors.Add(_playerColor);

                _normals.Add(Vector3.up);
                _normals.Add(Vector3.up);

                // Build quad triangles
                if (i < totalNodes - 1)
                {
                    int root = i * 2;
                    _triangles.Add(root);
                    _triangles.Add(root + 2);
                    _triangles.Add(root + 1);

                    _triangles.Add(root + 1);
                    _triangles.Add(root + 2);
                    _triangles.Add(root + 3);
                }
            }

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetColors(_colors);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetNormals(_normals);
            _mesh.RecalculateBounds();
        }

        public void ClearMesh()
        {
            if (_mesh != null)
            {
                _mesh.Clear();
            }
        }
    }
}
