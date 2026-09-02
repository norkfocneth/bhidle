using System;
using UnityEngine;

namespace TERRAGRAV.Trail
{
    /// <summary>
    /// Compact struct representing a discrete point along an active player trail.
    /// Memory footprint is kept minimal for low overhead.
    /// </summary>
    [Serializable]
    public struct TrailPoint
    {
        public Vector3 position;
        public Vector2Int gridCoord;
        public float timestamp;
        public int segmentIndex;

        public TrailPoint(Vector3 pos, Vector2Int grid, float time, int index)
        {
            position = pos;
            gridCoord = grid;
            timestamp = time;
            segmentIndex = index;
        }
    }
}
