using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Core;

namespace TERRAGRAV.Trail
{
    /// <summary>
    /// Global registry and spatial query engine for all active player and bot trails in the match.
    /// Enables O(1) tracking and efficient line-segment intersection queries.
    /// </summary>
    public class TrailManager : MonoBehaviour
    {
        private readonly List<PlayerTrail> _activeTrails = new List<PlayerTrail>();

        public IReadOnlyList<PlayerTrail> ActiveTrails => _activeTrails;

        private void Awake()
        {
            ServiceLocator.Register<TrailManager>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<TrailManager>();
        }

        public void RegisterTrail(PlayerTrail trail)
        {
            if (trail != null && !_activeTrails.Contains(trail))
            {
                _activeTrails.Add(trail);
            }
        }

        public void UnregisterTrail(PlayerTrail trail)
        {
            if (trail != null)
            {
                _activeTrails.Remove(trail);
            }
        }

        /// <summary>
        /// Checks if a given 3D position (attacker head) intersects any segment of a victim's active trail.
        /// </summary>
        public bool CheckTrailIntersection(Vector3 position, float radius, int attackerPlayerId, out int victimPlayerId, out int segmentIndex)
        {
            victimPlayerId = -1;
            segmentIndex = -1;
            float radiusSqr = radius * radius;

            for (int t = 0; t < _activeTrails.Count; t++)
            {
                PlayerTrail trail = _activeTrails[t];
                if (trail == null || trail.PointCount < 2) continue;

                // If checking self-intersection, ignore the most recent 3 segments directly behind the head
                int maxCheckIndex = (trail.PlayerId == attackerPlayerId) ? trail.PointCount - 4 : trail.PointCount;
                if (maxCheckIndex <= 0) continue;

                IReadOnlyList<TrailPoint> points = trail.Points;
                for (int i = 0; i < maxCheckIndex; i++)
                {
                    Vector3 p = points[i].position;
                    float distSqr = (position.x - p.x) * (position.x - p.x) + (position.z - p.z) * (position.z - p.z);

                    if (distSqr <= radiusSqr)
                    {
                        victimPlayerId = trail.PlayerId;
                        segmentIndex = i;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
