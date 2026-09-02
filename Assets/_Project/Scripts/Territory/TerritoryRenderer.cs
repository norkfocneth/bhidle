using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Core;

namespace TERRAGRAV.Territory
{
    /// <summary>
    /// Renders claimed territory as a raised 2.5D procedural mesh surface.
    /// Generates vertex-colored quads without creating per-cell GameObjects.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class TerritoryRenderer : MonoBehaviour
    {
        [Header("2.5D Surface Styling")]
        [Tooltip("Height of the territory surface above the floor plane.")]
        [SerializeField] private float _surfaceElevationY = 0.08f;

        [Tooltip("Material used to render the territory surface.")]
        [SerializeField] private Material _surfaceMaterial;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;
        private TerritoryGrid _grid;

        // Cached collections to prevent garbage allocation during 60 FPS mobile rendering
        private readonly List<Vector3> _vertices = new List<Vector3>(8192);
        private readonly List<int> _triangles = new List<int>(12288);
        private readonly List<Color> _colors = new List<Color>(8192);
        private readonly List<Vector3> _normals = new List<Vector3>(8192);

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _mesh = new Mesh
            {
                name = "TerritorySurface_Mesh"
            };
            _mesh.MarkDynamic();
            _meshFilter.sharedMesh = _mesh;

            if (_surfaceMaterial != null)
            {
                _meshRenderer.sharedMaterial = _surfaceMaterial;
            }
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out TerritoryGrid grid))
            {
                SetGrid(grid);
            }
        }

        private void OnDestroy()
        {
            if (_grid != null)
            {
                _grid.OnRegionModified -= HandleRegionModified;
            }
            if (_mesh != null)
            {
                Destroy(_mesh);
            }
        }

        public void SetGrid(TerritoryGrid grid)
        {
            if (_grid != null)
            {
                _grid.OnRegionModified -= HandleRegionModified;
            }

            _grid = grid;
            if (_grid != null)
            {
                _grid.OnRegionModified += HandleRegionModified;
                RebuildSurface();
            }
        }

        private void HandleRegionModified(int minX, int minY, int maxX, int maxY)
        {
            RebuildSurface();
        }

        /// <summary>
        /// Reconstructs the 2.5D raised mesh for all captured territory cells.
        /// </summary>
        public void RebuildSurface()
        {
            if (_grid == null || _grid.Data == null) return;

            _vertices.Clear();
            _triangles.Clear();
            _colors.Clear();
            _normals.Clear();

            int width = _grid.Width;
            int height = _grid.Height;
            float halfCell = _grid.CellSize * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TerritoryCell cell = _grid.GetCell(x, y);
                    if (!cell.isCaptured || cell.ownerId == TerritoryCell.UNCLAIMED) continue;

                    Color playerColor = _grid.Data.GetPlayerColor(cell.ownerId);
                    Vector3 center = _grid.GridToWorld(x, y, _surfaceElevationY);

                    int vertIndex = _vertices.Count;

                    // Add 4 vertices for top quad
                    _vertices.Add(new Vector3(center.x - halfCell, _surfaceElevationY, center.z - halfCell));
                    _vertices.Add(new Vector3(center.x - halfCell, _surfaceElevationY, center.z + halfCell));
                    _vertices.Add(new Vector3(center.x + halfCell, _surfaceElevationY, center.z + halfCell));
                    _vertices.Add(new Vector3(center.x + halfCell, _surfaceElevationY, center.z - halfCell));

                    for (int i = 0; i < 4; i++)
                    {
                        _colors.Add(playerColor);
                        _normals.Add(Vector3.up);
                    }

                    // 2 triangles
                    _triangles.Add(vertIndex);
                    _triangles.Add(vertIndex + 1);
                    _triangles.Add(vertIndex + 2);

                    _triangles.Add(vertIndex);
                    _triangles.Add(vertIndex + 2);
                    _triangles.Add(vertIndex + 3);
                }
            }

            _mesh.Clear();
            if (_vertices.Count > 0)
            {
                _mesh.SetVertices(_vertices);
                _mesh.SetTriangles(_triangles, 0);
                _mesh.SetColors(_colors);
                _mesh.SetNormals(_normals);
                _mesh.RecalculateBounds();
            }
        }
    }
}
