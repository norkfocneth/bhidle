using UnityEngine;
using TERRAGRAV.Core;

namespace TERRAGRAV.CameraSystem
{
    /// <summary>
    /// 2.5D Orthographic Camera Controller providing smooth tracking of the player.
    /// Uses a 60-degree downward angle and subtle territory-based dynamic zoom without jitter.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class GameCameraController : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 25f, -15f);

        [Tooltip("Smooth damping time for following player position.")]
        [SerializeField] private float _followSmoothTime = 0.12f;

        [Header("2.5D Perspective Angles")]
        [Tooltip("Downward pitch angle in degrees (60 degrees for 2.5D mobile look).")]
        [SerializeField] private float _pitchAngle = 60f;
        [SerializeField] private float _yawAngle = 0f;

        [Header("Dynamic Orthographic Zoom")]
        [SerializeField] private float _orthographicSize = 16f;
        [SerializeField] private float _minimumZoom = 15f;
        [SerializeField] private float _maximumZoom = 17f;
        [SerializeField] private float _zoomSmoothTime = 0.5f;

        private Camera _camera;
        private Vector3 _currentVelocity;
        private float _targetOrthoSize;
        private float _zoomVelocity;

        public Camera UnityCamera => _camera;
        public Transform Target => _target;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
            _targetOrthoSize = _orthographicSize;
            _camera.orthographicSize = _orthographicSize;

            transform.rotation = Quaternion.Euler(_pitchAngle, _yawAngle, 0f);
            ServiceLocator.Register<GameCameraController>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameCameraController>();
        }

        private float _shakeIntensity = 0f;
        private float _shakeDuration = 0f;

        private void OnEnable()
        {
            GameEvents.OnTerritoryChanged += HandleTerritoryChanged;
            GameEvents.OnCameraShake += HandleCameraShake;
        }

        private void OnDisable()
        {
            GameEvents.OnTerritoryChanged -= HandleTerritoryChanged;
            GameEvents.OnCameraShake -= HandleCameraShake;
        }

        public void SetTarget(Transform targetTransform)
        {
            _target = targetTransform;
            if (_target != null)
            {
                transform.position = _target.position + _offset;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // 1. Smooth positional follow
            Vector3 targetPosition = _target.position + _offset;

            // Apply camera shake if active
            if (_shakeDuration > 0f)
            {
                _shakeDuration -= Time.deltaTime;
                Vector3 shakeOffset = Random.insideUnitSphere * _shakeIntensity;
                targetPosition += shakeOffset;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, _followSmoothTime);

            // 2. Subtle dynamic zoom smoothing
            _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, _targetOrthoSize, ref _zoomVelocity, _zoomSmoothTime);
        }

        private void HandleCameraShake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
        }

        private void HandleTerritoryChanged(int playerId, int cellCount, float percentage)
        {
            // Subtle zoom scaling from min to max based on territory percentage (0% to 25%)
            float t = Mathf.Clamp01(percentage / 25f);
            _targetOrthoSize = Mathf.Lerp(_minimumZoom, _maximumZoom, t);
        }
    }
}
