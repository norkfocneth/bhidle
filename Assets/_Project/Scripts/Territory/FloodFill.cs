using System;
using UnityEngine;

namespace TERRAGRAV.Territory
{
    /// <summary>
    /// High-performance 2D Bounded Flood-Fill algorithm for territory enclosure calculations.
    /// Uses pre-allocated static buffers to eliminate garbage collection during runtime captures.
    /// </summary>
    public class FloodFill
    {
        private const int MAX_GRID_CELLS = 250 * 250;

        // Reusable static buffers to avoid GC allocations during mobile gameplay
        private static readonly byte[] _visited = new byte[MAX_GRID_CELLS];
        private static readonly byte[] _trailMask = new byte[MAX_GRID_CELLS];
        private static readonly int[] _queueX = new int[MAX_GRID_CELLS];
        private static readonly int[] _queueY = new int[MAX_GRID_CELLS];

        /// <summary>
        /// Clears the trail mask buffer for a given sub-region.
        /// </summary>
        public static void ClearTrailMask(int minX, int minY, int maxX, int maxY, int gridWidth)
        {
            for (int y = minY; y <= maxY; y++)
            {
                int rowOffset = y * gridWidth;
                for (int x = minX; x <= maxX; x++)
                {
                    _trailMask[rowOffset + x] = 0;
                }
            }
        }

        /// <summary>
        /// Marks a grid cell as part of the active trail boundary in the mask.
        /// </summary>
        public static void MarkTrailCell(int gx, int gy, int gridWidth)
        {
            _trailMask[gy * gridWidth + gx] = 1;
        }

        /// <summary>
        /// Executes a bounded flood-fill starting from the bounding box perimeter.
        /// Identifies all interior cells unreachable from the outside boundary.
        /// </summary>
        /// <returns>Number of interior enclosed cells identified.</returns>
        public static int ExecuteEnclosureFill(
            TerritoryGrid grid,
            int playerId,
            int minX, int minY, int maxX, int maxY,
            Action<int, int> onEnclosedCellFound)
        {
            int width = grid.Width;
            int height = grid.Height;

            // 1. Clear visited buffer within the bounding box
            for (int y = minY; y <= maxY; y++)
            {
                int rowOffset = y * width;
                for (int x = minX; x <= maxX; x++)
                {
                    _visited[rowOffset + x] = 0;
                }
            }

            int queueHead = 0;
            int queueTail = 0;

            // Helper local action to enqueue boundary seed cells
            void Enqueue(int qx, int qy)
            {
                int idx = qy * width + qx;
                if (_visited[idx] == 0)
                {
                    _visited[idx] = 1;
                    _queueX[queueTail] = qx;
                    _queueY[queueTail] = qy;
                    queueTail++;
                }
            }

            // 2. Push all 4 outer edges of the sub-bounding box as flood-fill seeds
            for (int x = minX; x <= maxX; x++)
            {
                Enqueue(x, minY);
                Enqueue(x, maxY);
            }
            for (int y = minY; y <= maxY; y++)
            {
                Enqueue(minX, y);
                Enqueue(maxX, y);
            }

            // 3. Breadth-First Search (BFS) Flood Fill
            while (queueHead < queueTail)
            {
                int cx = _queueX[queueHead];
                int cy = _queueY[queueHead];
                queueHead++;

                // 4-way cardinal neighbors
                TryVisitNeighbor(grid, playerId, cx + 1, cy, minX, minY, maxX, maxY, width, ref queueTail);
                TryVisitNeighbor(grid, playerId, cx - 1, cy, minX, minY, maxX, maxY, width, ref queueTail);
                TryVisitNeighbor(grid, playerId, cx, cy + 1, minX, minY, maxX, maxY, width, ref queueTail);
                TryVisitNeighbor(grid, playerId, cx, cy - 1, minX, minY, maxX, maxY, width, ref queueTail);
            }

            // 4. Collect all unreachable cells inside the bounding box (the enclosed polygon interior)
            int enclosedCount = 0;
            for (int y = minY; y <= maxY; y++)
            {
                int rowOffset = y * width;
                for (int x = minX; x <= maxX; x++)
                {
                    int idx = rowOffset + x;
                    if (_visited[idx] == 0)
                    {
                        onEnclosedCellFound?.Invoke(x, y);
                        enclosedCount++;
                    }
                }
            }

            return enclosedCount;
        }

        private static void TryVisitNeighbor(
            TerritoryGrid grid,
            int playerId,
            int nx, int ny,
            int minX, int minY, int maxX, int maxY,
            int width,
            ref int queueTail)
        {
            if (nx < minX || nx > maxX || ny < minY || ny > maxY) return;

            int nIdx = ny * width + nx;
            if (_visited[nIdx] != 0) return;

            // Barrier condition: Trail line or player's existing territory
            bool isTrail = (_trailMask[nIdx] == 1);
            TerritoryCell cell = grid.GetCell(nx, ny);
            bool isOwnTerritory = (cell.ownerId == playerId && cell.isCaptured);

            if (!isTrail && !isOwnTerritory)
            {
                _visited[nIdx] = 1;
                _queueX[queueTail] = nx;
                _queueY[queueTail] = ny;
                queueTail++;
            }
        }
    }
}
