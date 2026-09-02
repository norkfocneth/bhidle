using System.Collections.Generic;
using UnityEngine;

namespace TERRAGRAV.Territory
{
    /// <summary>
    /// Logical storage for territory cell ownership and real-time statistics calculation.
    /// Operates on the main Unity thread without unnecessary synchronization overhead.
    /// </summary>
    public class TerritoryData
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _totalCells;
        private readonly TerritoryCell[] _cells;
        private readonly Dictionary<int, int> _playerCellCounts = new Dictionary<int, int>();
        private readonly Dictionary<int, Color> _playerColors = new Dictionary<int, Color>();

        public int Width => _width;
        public int Height => _height;
        public int TotalCells => _totalCells;

        public TerritoryData(int width, int height)
        {
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
            _totalCells = _width * _height;

            _cells = new TerritoryCell[_totalCells];
            for (int i = 0; i < _totalCells; i++)
            {
                _cells[i] = TerritoryCell.CreateEmpty();
            }
        }

        public bool IsInsideBounds(int gx, int gy)
        {
            return gx >= 0 && gx < _width && gy >= 0 && gy < _height;
        }

        public int GetIndex(int gx, int gy)
        {
            return gy * _width + gx;
        }

        public TerritoryCell GetCell(int gx, int gy)
        {
            if (!IsInsideBounds(gx, gy))
            {
                return TerritoryCell.CreateEmpty();
            }
            return _cells[GetIndex(gx, gy)];
        }

        public void SetCell(int gx, int gy, TerritoryCell cell)
        {
            if (!IsInsideBounds(gx, gy)) return;

            int index = GetIndex(gx, gy);
            int previousOwner = _cells[index].ownerId;
            int newOwner = cell.ownerId;

            if (previousOwner != newOwner)
            {
                if (previousOwner != TerritoryCell.UNCLAIMED)
                {
                    ModifyPlayerCellCount(previousOwner, -1);
                }
                if (newOwner != TerritoryCell.UNCLAIMED)
                {
                    ModifyPlayerCellCount(newOwner, 1);
                }
            }

            _cells[index] = cell;
        }

        public void RegisterPlayer(int playerId, Color playerColor)
        {
            if (!_playerCellCounts.ContainsKey(playerId))
            {
                _playerCellCounts[playerId] = 0;
            }
            _playerColors[playerId] = playerColor;
        }

        private void ModifyPlayerCellCount(int playerId, int delta)
        {
            if (_playerCellCounts.ContainsKey(playerId))
            {
                _playerCellCounts[playerId] = Mathf.Max(0, _playerCellCounts[playerId] + delta);
            }
            else
            {
                _playerCellCounts[playerId] = Mathf.Max(0, delta);
            }
        }

        public int GetPlayerCellCount(int playerId)
        {
            return _playerCellCounts.TryGetValue(playerId, out int count) ? count : 0;
        }

        public float GetPlayerPercentage(int playerId)
        {
            if (_totalCells <= 0) return 0f;
            int count = GetPlayerCellCount(playerId);
            return ((float)count / _totalCells) * 100f;
        }

        public Color GetPlayerColor(int playerId)
        {
            return _playerColors.TryGetValue(playerId, out Color color) ? color : Color.white;
        }

        public void Reset()
        {
            for (int i = 0; i < _totalCells; i++)
            {
                _cells[i] = TerritoryCell.CreateEmpty();
            }
            _playerCellCounts.Clear();
            _playerColors.Clear();
        }
    }
}
