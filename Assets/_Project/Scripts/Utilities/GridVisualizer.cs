using UnityEngine;
using TERRAGRAV.Territory;

namespace TERRAGRAV.Utilities
{
    /// <summary>
    /// Editor development utility for drawing debug gizmos of the 2.5D territory grid and boundary bounds.
    /// Automatically disabled in production builds.
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [SerializeField] private bool _drawGridBounds = true;
        [SerializeField] private Color _boundsColor = new Color(0.2f, 0.8f, 1f, 0.4f);

        [SerializeField] private TerritoryGrid _grid;

        private void OnDrawGizmos()
        {
            if (!_drawGridBounds) return;

            if (_grid == null)
            {
                _grid = GetComponent<TerritoryGrid>();
                if (_grid == null) return;
            }

            float totalWidth = _grid.Width * _grid.CellSize;
            float totalHeight = _grid.Height * _grid.CellSize;

            Gizmos.color = _boundsColor;
            Gizmos.DrawWireCube(_grid.WorldOrigin, new Vector3(totalWidth, 1f, totalHeight));
        }
    }
}
