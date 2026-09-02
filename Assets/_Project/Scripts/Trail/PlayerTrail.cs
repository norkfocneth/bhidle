using System;
using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Territory;

namespace TERRAGRAV.Trail
{
    /// <summary>
    /// Manages the recording, sampling, and lifecycle of a player's active ribbon trail.
    /// Detects when the player transitions between inside and outside their territory.
    /// </summary>
    public class PlayerTrail : MonoBehaviour
    {
        [Header("Sampling Settings")]
        [Tooltip("Minimum distance in world units between successive trail points to prevent dense over-sampling.")]
        [SerializeField] private float _minSampleDistance = 0.35f;

        [Header("Components")]
        [SerializeField] private TrailRendererController _meshRendererController;

        private int _playerId = 1;
        private bool _isOutside = false;
        private bool _wasOutside = false;
        private TerritoryGrid _territoryGrid;

        private readonly List<TrailPoint> _points = new List<TrailPoint>(512);

        public int PlayerId => _playerId;
        public bool IsOutside => _isOutside;
        public IReadOnlyList<TrailPoint> Points => _points;
        public int PointCount => _points.Count;

        public event Action<int> OnTrailStarted;
        public event Action<int, TrailPoint> OnTrailPointAdded;
        public event Action<int, IReadOnlyList<TrailPoint>> OnTrailClosed;
        public event Action<int> OnTrailCleared;

        private void Awake()
        {
            if (_meshRendererController == null)
            {
                _meshRendererController = GetComponentInChildren<TrailRendererController>();
            }
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out TerritoryGrid grid))
            {
                _territoryGrid = grid;
            }
        }

        public void Initialize(int playerId, Color playerColor, float trailWidth, TerritoryGrid grid)
        {
            _playerId = playerId;
            _territoryGrid = grid;

            if (_meshRendererController != null)
            {
                _meshRendererController.SetColor(playerColor);
                _meshRendererController.SetWidth(trailWidth);
            }

            // Register with TrailManager
            if (ServiceLocator.TryGet(out TrailManager manager))
            {
                manager.RegisterTrail(this);
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out TrailManager manager))
            {
                manager.UnregisterTrail(this);
            }
        }

        private void Update()
        {
            if (_territoryGrid == null)
            {
                ServiceLocator.TryGet(out _territoryGrid);
                if (_territoryGrid == null) return;
            }

            Vector3 currentPos = transform.position;

            // 1. Query territory ownership at current player position
            if (_territoryGrid.WorldToGrid(currentPos, out int gx, out int gy))
            {
                TerritoryCell cell = _territoryGrid.GetCell(gx, gy);
                _isOutside = (cell.ownerId != _playerId);
            }
            else
            {
                _isOutside = true;
            }

            // 2. Handle State Transitions
            if (_isOutside)
            {
                // Transition: Just stepped outside
                if (!_wasOutside)
                {
                    _points.Clear();
                    TrailPoint firstPoint = new TrailPoint(currentPos, new Vector2Int(gx, gy), Time.time, 0);
                    _points.Add(firstPoint);
                    OnTrailStarted?.Invoke(_playerId);
                    GameEvents.TriggerTrailStarted(_playerId);
                }
                else
                {
                    // Sample additional points if distance threshold is exceeded
                    if (_points.Count > 0)
                    {
                        TrailPoint lastPoint = _points[_points.Count - 1];
                        float distSqr = (currentPos.x - lastPoint.position.x) * (currentPos.x - lastPoint.position.x) +
                                        (currentPos.z - lastPoint.position.z) * (currentPos.z - lastPoint.position.z);

                        if (distSqr >= (_minSampleDistance * _minSampleDistance))
                        {
                            TrailPoint newPoint = new TrailPoint(currentPos, new Vector2Int(gx, gy), Time.time, _points.Count);
                            _points.Add(newPoint);
                            OnTrailPointAdded?.Invoke(_playerId, newPoint);
                            GameEvents.TriggerTrailPointAdded(_playerId, newPoint);
                        }
                    }
                }
            }
            else
            {
                // Transition: Just returned inside territory with a recorded trail
                if (_wasOutside && _points.Count >= 2)
                {
                    TrailPoint finalPoint = new TrailPoint(currentPos, new Vector2Int(gx, gy), Time.time, _points.Count);
                    _points.Add(finalPoint);
                    OnTrailClosed?.Invoke(_playerId, _points);
                    GameEvents.TriggerTrailClosed(_playerId, _points);
                }

                _points.Clear();
            }

            _wasOutside = _isOutside;
        }

        private void LateUpdate()
        {
            // Update procedural 2.5D ribbon mesh
            if (_meshRendererController != null)
            {
                _meshRendererController.UpdateMesh(_points, transform.position, _isOutside);
            }
        }

        /// <summary>
        /// Clears all recorded trail points and resets the procedural mesh.
        /// </summary>
        public void ClearTrail()
        {
            _points.Clear();
            _isOutside = false;
            _wasOutside = false;

            if (_meshRendererController != null)
            {
                _meshRendererController.ClearMesh();
            }

            OnTrailCleared?.Invoke(_playerId);
        }
    }
}
