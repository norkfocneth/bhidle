using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Trail;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Synchronizes dynamic trail point deltas across the network.
    /// Broadcasts new point additions incrementally to conserve network bandwidth.
    /// </summary>
    public class NetworkTrail : MonoBehaviour
    {
        private PlayerTrail _localTrail;
        private int _lastSyncedPointCount = 0;

        private void Awake()
        {
            _localTrail = GetComponent<PlayerTrail>();
        }

        /// <summary>
        /// Collects newly added trail points since the last network tick.
        /// </summary>
        public List<TrailPoint> GetUnsyncedPoints()
        {
            var newPoints = new List<TrailPoint>();
            if (_localTrail == null) return newPoints;

            var points = _localTrail.Points;
            for (int i = _lastSyncedPointCount; i < points.Count; i++)
            {
                newPoints.Add(points[i]);
            }

            _lastSyncedPointCount = points.Count;
            return newPoints;
        }

        /// <summary>
        /// Ingests remote trail points received from network packet.
        /// </summary>
        public void ApplyRemotePoints(IReadOnlyList<TrailPoint> remotePoints)
        {
            // Update procedural mesh for remote player
            if (_localTrail != null && _localTrail.MeshController != null)
            {
                _localTrail.MeshController.RebuildRibbonMesh(remotePoints);
            }
        }

        /// <summary>
        /// Resets sync counter when a trail is cleared or closed.
        /// </summary>
        public void ResetSync()
        {
            _lastSyncedPointCount = 0;
        }
    }
}
