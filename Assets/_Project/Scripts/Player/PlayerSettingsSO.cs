using UnityEngine;

namespace TERRAGRAV.Player
{
    /// <summary>
    /// Configuration data for player movement physics, turning agility, dimensions, and visual parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "TERRAGRAV/Player/Player Settings")]
    public class PlayerSettingsSO : ScriptableObject
    {
        [Header("Locomotion")]
        [Tooltip("Base continuous forward movement speed in world units per second.")]
        [SerializeField] private float _movementSpeed = 12.0f;

        [Header("Locomotion Attributes")]
        [Tooltip("Standard movement speed in world units per second.")]
        [SerializeField] private float _baseSpeed = 8.0f;

        [Tooltip("Maximum movement speed when boosting.")]
        [SerializeField] private float _boostSpeed = 10.0f;

        [Tooltip("Angular steering rate in degrees per second.")]
        [SerializeField] private float _turnSpeed = 360.0f;

        [Header("Territory Parameters")]
        [Tooltip("Radius of the initial circular claimed base in world units (15 units = ~0.44% of map).")]
        [SerializeField] private float _startingTerritoryRadius = 15.0f;

        [Tooltip("Saturated faction color used to render captured territory.")]
        [SerializeField] private Color _captureColor = new Color(0.1f, 0.45f, 0.91f); // Royal Blue

        [Header("Arena Bounds")]
        [Tooltip("Absolute X and Z world boundaries (-200 to +200).")]
        [SerializeField] private float _boundaryLimit = 195.0f;

        [Header("Dimensions & Physics")]
        [Tooltip("Elevation height of the player model on the Y-axis.")]
        [SerializeField] private float _playerHeight = 0.5f;

        [Tooltip("Visual and logical width of the player trail.")]
        [SerializeField] private float _trailWidth = 0.6f;

        [SerializeField] private Color _captureColor = new Color(0.14f, 0.58f, 0.98f, 1f);

        // Public Properties
        public float MovementSpeed => _movementSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float TurnSpeed => _turnSpeed;
        public float StartingTerritoryRadius => _startingTerritoryRadius;
        public float PlayerHeight => _playerHeight;
        public float TrailWidth => _trailWidth;
        public float BoundaryLimit => _boundaryLimit;
        public Color CaptureColor => _captureColor;
    }
}
