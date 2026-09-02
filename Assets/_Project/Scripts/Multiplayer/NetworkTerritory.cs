using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Territory;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Synchronizes territory capture events via delta bounding box messages.
    /// Eliminates full-grid broadcast overhead by transmitting only claimed cell coordinates.
    /// </summary>
    public class NetworkTerritory : MonoBehaviour
    {
        private TerritoryGrid _grid;

        private void Start()
        {
            if (ServiceLocator.TryGet(out TerritoryGrid grid))
            {
                _grid = grid;
            }
        }

        /// <summary>
        /// Applies an authoritative capture operation received from the server.
        /// </summary>
        public void ApplyRemoteCapture(int playerId, int minX, int minY, int maxX, int maxY, List<Vector2Int> claimedCells)
        {
            if (_grid == null)
            {
                ServiceLocator.TryGet(out _grid);
                if (_grid == null) return;
            }

            for (int i = 0; i < claimedCells.Count; i++)
            {
                Vector2Int c = claimedCells[i];
                _grid.SetCell(c.x, c.y, new TerritoryCell(playerId, 0));
            }

            _grid.NotifyRegionModified(minX, minY, maxX, maxY);
        }

        /// <summary>
        /// Wipes territory for a disconnected or eliminated player.
        /// </summary>
        public void ApplyRemoteTerritoryWipe(int playerId)
        {
            if (_grid == null)
            {
                ServiceLocator.TryGet(out _grid);
                if (_grid == null) return;
            }

            int w = _grid.Width;
            int h = _grid.Height;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (_grid.GetCell(x, y).ownerId == playerId)
                    {
                        _grid.SetCell(x, y, TerritoryCell.CreateEmpty());
                    }
                }
            }

            _grid.NotifyRegionModified(0, 0, w - 1, h - 1);
        }
    }
}
