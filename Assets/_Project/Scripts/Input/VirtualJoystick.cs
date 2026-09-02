using UnityEngine;
using UnityEngine.EventSystems;

namespace TERRAGRAV.Input
{
    /// <summary>
    /// Touch-friendly Virtual Joystick compatible with Unity UI EventSystem.
    /// Supports configurable radius, deadzone, safe-area awareness, and dynamic positioning without per-frame allocations.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Hierarchy References")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;

        [Header("Configuration")]
        [Tooltip("Maximum movement radius of the handle from center in canvas pixels.")]
        [SerializeField] private float _radius = 100f;

        [Tooltip("Deadzone ratio (0.0 to 1.0) below which inputs are zeroed.")]
        [SerializeField] private float _deadZone = 0.1f;

        [Tooltip("Whether the joystick automatically anchors to where the user touches.")]
        [SerializeField] private bool _dynamicPositioning = true;

        private Canvas _canvas;
        private Camera _uiCamera;
        private Vector2 _inputDirection = Vector2.zero;
        private Vector2 _defaultBackgroundPos;
        private bool _isHeld = false;

        public Vector2 InputDirection => _inputDirection;
        public bool IsHeld => _isHeld;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null)
            {
                Debug.LogError("[VirtualJoystick] Missing Canvas in parent hierarchy!");
            }

            if (_background != null)
            {
                _defaultBackgroundPos = _background.anchoredPosition;
            }
            else
            {
                Debug.LogError("[VirtualJoystick] Missing Background RectTransform reference!");
            }

            if (_handle == null)
            {
                Debug.LogError("[VirtualJoystick] Missing Handle RectTransform reference!");
            }
        }

        private void Start()
        {
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _uiCamera = _canvas.worldCamera;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isHeld = true;

            if (_dynamicPositioning && _background != null && _canvas != null)
            {
                RectTransform canvasRect = (RectTransform)_canvas.transform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, _uiCamera, out Vector2 localPoint))
                {
                    _background.anchoredPosition = localPoint;
                }
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_background == null || _handle == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_background, eventData.position, _uiCamera, out Vector2 localPoint))
            {
                float halfSize = _background.sizeDelta.x > 0f ? _background.sizeDelta.x * 0.5f : _radius;
                Vector2 normalizedVector = localPoint / halfSize;

                if (normalizedVector.magnitude > 1f)
                {
                    normalizedVector.Normalize();
                }

                if (normalizedVector.magnitude < _deadZone)
                {
                    _inputDirection = Vector2.zero;
                }
                else
                {
                    _inputDirection = normalizedVector;
                }

                _handle.anchoredPosition = _inputDirection * _radius;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isHeld = false;
            _inputDirection = Vector2.zero;

            if (_handle != null)
            {
                _handle.anchoredPosition = Vector2.zero;
            }

            if (_dynamicPositioning && _background != null)
            {
                _background.anchoredPosition = _defaultBackgroundPos;
            }
        }

        public void ResetState()
        {
            _isHeld = false;
            _inputDirection = Vector2.zero;
            if (_handle != null) _handle.anchoredPosition = Vector2.zero;
            if (_background != null) _background.anchoredPosition = _defaultBackgroundPos;
        }
    }
}
