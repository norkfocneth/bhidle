using System;
using UnityEngine;
using TERRAGRAV.Core;

namespace TERRAGRAV.Territory
{
    /// <summary>
    /// Logical 200x200 grid representing the territory arena.
    /// Provides spatial mapping between 3D world space coordinates and discrete 2D grid cells.
    /// </summary>
    public class TerritoryGrid : MonoBehaviour
    {
        [Header("Grid Dimensions")]
        [SerializeField] private int _width = 400;
        [SerializeField] private int _height = 400;
        [SerializeField] private float _cellSize = 1.0f;

        [Header("World Origin Mapping")]
        [Tooltip("World coordinate representing the center or bottom-left origin of the grid.")]
        [SerializeField] private Vector3 _worldOrigin = Vector3.zero;
        [SerializeField] private bool _centerOriginOnWorldPos = true;

        private TerritoryData _data;

        public int Width => _width;
        public int Height => _height;
        public float CellSize => _cellSize;
        public Vector3 WorldOrigin => _worldOrigin;
        public TerritoryData Data => _data;

        public event Action<int, int, int, int> OnRegionModified; // minX, minY, maxX, maxY

        private void Awake()
        {
            InitializeGrid();
            ServiceLocator.Register<TerritoryGrid>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TerritoryGrid>();
        }

        public void InitializeGrid()
        {
            _data = new TerritoryData(_width, _height);
        }

        public bool IsInsideGrid(int gx, int gy)
        {
            return _data != null && _data.IsInsideBounds(gx, gy);
        }

        public bool IsInsideGrid(Vector2Int gridCoord)
        {
            return IsInsideGrid(gridCoord.x, gridCoord.y);
        }

        public TerritoryCell GetCell(int gx, int gy)
        {
            if (_data == null) return TerritoryCell.CreateEmpty();
            return _data.GetCell(gx, gy);
        }

        public TerritoryCell GetCell(Vector2Int gridCoord)
        {
            return GetCell(gridCoord.x, gridCoord.y);
        }

        public void SetCell(int gx, int gy, TerritoryCell cell)
        {
            if (_data == null || !IsInsideGrid(gx, gy)) return;
            _data.SetCell(gx, gy, cell);
        }

        public void SetCell(Vector2Int gridCoord, TerritoryCell cell)
        {
            SetCell(gridCoord.x, gridCoord.y, cell);
        }

        /// <summary>
        /// Transforms 3D world coordinates into 2D discrete grid coordinates.
        /// </summary>
        public bool WorldToGrid(Vector3 worldPosition, out int gx, out int gy)
        {
            float originOffsetX = _centerOriginOnWorldPos ? (_width * _cellSize * 0.5f) : 0f;
            float originOffsetZ = _centerOriginOnWorldPos ? (_height * _cellSize * 0.5f) : 0f;

            float relX = worldPosition.x - _worldOrigin.x + originOffsetX;
            float relZ = worldPosition.z - _worldOrigin.z + originOffsetZ;

            gx = Mathf.FloorToInt(relX / _cellSize);
            gy = Mathf.FloorToInt(relZ / _cellSize);

            return IsInsideGrid(gx, gy);
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            WorldToGrid(worldPosition, out int gx, out int gy);
            return new Vector2Int(gx, gy);
        }

        /// <summary>
        /// Transforms 2D discrete grid coordinates into 3D world center position.
        /// </summary>
        public Vector3 GridToWorld(int gx, int gy, float elevationY = 0f)
        {
            float originOffsetX = _centerOriginOnWorldPos ? (_width * _cellSize * 0.5f) : 0f;
            float originOffsetZ = _centerOriginOnWorldPos ? (_height * _cellSize * 0.5f) : 0f;

            float worldX = (gx * _cellSize) + (_cellSize * 0.5f) - originOffsetX + _worldOrigin.x;
            float worldZ = (gy * _cellSize) + (_cellSize * 0.5f) - originOffsetZ + _worldOrigin.z;

            return new Vector3(worldX, elevationY, worldZ);
        }

        public Vector3 GridToWorld(Vector2Int gridCoord, float elevationY = 0f)
        {
            return GridToWorld(gridCoord.x, gridCoord.y, elevationY);
        }

        /// <summary>
        /// Claims a starting circular base for a player on initialization.
        /// </summary>
        public void ClaimStartingTerritory(int playerId, Vector3 centerWorldPos, float radiusWorld, Color playerColor)
        {
            if (_data == null) InitializeGrid();
            _data.RegisterPlayer(playerId, playerColor);

            if (!WorldToGrid(centerWorldPos, out int centerX, out int centerY))
            {
                Debug.LogError($"[TerritoryGrid] Starting territory center {centerWorldPos} is outside grid bounds!");
                return;
            }

            int cellRadius = Mathf.CeilToInt(radiusWorld / _cellSize);
            int minX = Mathf.Max(0, centerX - cellRadius);
            int maxX = Mathf.Min(_width - 1, centerX + cellRadius);
            int minY = Mathf.Max(0, centerY - cellRadius);
            int maxY = Mathf.Min(_height - 1, centerY + cellRadius);

            float radiusSqr = (radiusWorld / _cellSize) * (radiusWorld / _cellSize);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float dx = x - centerX;
                    float dy = y - centerY;
                    if ((dx * dx) + (dy * dy) <= radiusSqr)
                    {
                        TerritoryCell cell = new TerritoryCell
                        {
                            ownerId = playerId,
                            isCaptured = true,
                            isBoundary = false
                        };
                        _data.SetCell(x, y, cell);
                    }
                }
            }

            OnRegionModified?.Invoke(minX, minY, maxX, maxY);
        }

        public void NotifyRegionModified(int minX, int minY, int maxX, int maxY)
        {
            OnRegionModified?.Invoke(minX, minY, maxX, maxY);
        }
    }
}
