using System;
using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Trail;

namespace TERRAGRAV.Territory
{
    /// <summary>
    /// Master territory capture engine.
    /// Converts closed trail loops into permanent captured territory cells using bounded flood-fill.
    /// </summary>
    public class TerritoryCapture : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TerritoryGrid _grid;

        private void Awake()
        {
            ServiceLocator.Register<TerritoryCapture>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TerritoryCapture>();
        }

        private void OnEnable()
        {
            GameEvents.OnTrailClosed += HandleTrailClosed;
        }

        private void OnDisable()
        {
            GameEvents.OnTrailClosed -= HandleTrailClosed;
        }

        private void Start()
        {
            if (_grid == null)
            {
                ServiceLocator.TryGet(out _grid);
            }
        }

        /// <summary>
        /// Captures the enclosed polygon area defined by the closed trail.
        /// </summary>
        public int ProcessTrailCapture(int playerId, IReadOnlyList<TrailPoint> points)
        {
            if (_grid == null || points == null || points.Count < 2) return 0;

            int gridWidth = _grid.Width;
            int gridHeight = _grid.Height;

            int minGx = gridWidth - 1;
            int maxGx = 0;
            int minGy = gridHeight - 1;
            int maxGy = 0;

            // 1. Calculate Bounding Box of the trail
            for (int i = 0; i < points.Count; i++)
            {
                Vector2Int gc = points[i].gridCoord;
                if (gc.x < minGx) minGx = gc.x;
                if (gc.x > maxGx) maxGx = gc.x;
                if (gc.y < minGy) minGy = gc.y;
                if (gc.y > maxGy) maxGy = gc.y;
            }

            // Apply bounding box padding
            const int PADDING = 4;
            minGx = Mathf.Max(0, minGx - PADDING);
            maxGx = Mathf.Min(gridWidth - 1, maxGx + PADDING);
            minGy = Mathf.Max(0, minGy - PADDING);
            maxGy = Mathf.Min(gridHeight - 1, maxGy + PADDING);

            // 2. Clear trail mask for sub-bounding box
            FloodFill.ClearTrailMask(minGx, minGy, maxGx, maxGy, gridWidth);

            // 3. Rasterize continuous lines between consecutive trail points
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2Int p0 = points[i].gridCoord;
                Vector2Int p1 = points[i + 1].gridCoord;

                RasterizeLine(p0.x, p0.y, p1.x, p1.y, (x, y) =>
                {
                    if (_grid.IsInsideGrid(x, y))
                    {
                        FloodFill.MarkTrailCell(x, y, gridWidth);
                    }
                });
            }

            // 4. Execute Enclosure Flood-Fill and assign claimed cells
            int capturedCells = 0;
            FloodFill.ExecuteEnclosureFill(_grid, playerId, minGx, minGy, maxGx, maxGy, (x, y) =>
            {
                TerritoryCell newCell = new TerritoryCell
                {
                    ownerId = playerId,
                    isCaptured = true,
                    isBoundary = false
                };
                _grid.SetCell(x, y, newCell);
                capturedCells++;
            });

            // 5. Notify systems & update metrics
            if (capturedCells > 0)
            {
                _grid.NotifyRegionModified(minGx, minGy, maxGx, maxGy);

                int totalOwned = _grid.Data.GetPlayerCellCount(playerId);
                float percentage = _grid.Data.GetPlayerPercentage(playerId);

                GameEvents.TriggerTerritoryChanged(playerId, totalOwned, percentage);
                GameEvents.TriggerPlayerCapturedTerritory(playerId, capturedCells);
            }

            return capturedCells;
        }

        private void HandleTrailClosed(int playerId, IReadOnlyList<TrailPoint> points)
        {
            ProcessTrailCapture(playerId, points);
        }

        /// <summary>
        /// Bresenham's line algorithm for continuous grid cell rasterization without diagonal gaps.
        /// </summary>
        private static void RasterizeLine(int x0, int y0, int x1, int y1, Action<int, int> plot)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                plot(x0, y0);
                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}
