using UnityEngine;
using TERRAGRAV.Core;

namespace TERRAGRAV.Input
{
    /// <summary>
    /// Central manager responsible for choosing and instantiating the active IInputProvider.
    /// Ensures player components never directly read platform-specific hardware APIs.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Mobile References")]
        [SerializeField] private VirtualJoystick _virtualJoystick;

        [Header("Editor & Platform Overrides")]
        [Tooltip("Force mobile virtual joystick provider inside the Unity Editor.")]
        [SerializeField] private bool _forceMobileInEditor = false;

        [Tooltip("Enable mouse cursor steering when running standalone PC provider.")]
        [SerializeField] private bool _enableMouseSteering = true;

        private IInputProvider _activeProvider;

        public IInputProvider ActiveProvider => _activeProvider;

        private void Awake()
        {
            ServiceLocator.Register<InputManager>(this);
            SetupInputProvider();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<InputManager>();
        }

        private void SetupInputProvider()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (_virtualJoystick == null)
            {
                Debug.LogWarning("[InputManager] Mobile platform detected but VirtualJoystick reference is missing. Searching in scene...");
                _virtualJoystick = FindObjectOfType<VirtualJoystick>();
            }
            _activeProvider = new MobileInputProvider(_virtualJoystick);
#else
            if (_forceMobileInEditor)
            {
                if (_virtualJoystick == null)
                {
                    _virtualJoystick = FindObjectOfType<VirtualJoystick>();
                }
                _activeProvider = new MobileInputProvider(_virtualJoystick);
            }
            else
            {
                _activeProvider = new StandaloneInputProvider(_enableMouseSteering);
            }
#endif
        }

        /// <summary>
        /// Returns the movement heading provided by the active input strategy.
        /// </summary>
        public Vector2 GetMovementDirection()
        {
            if (_activeProvider == null) SetupInputProvider();
            return _activeProvider != null ? _activeProvider.GetMovementDirection() : Vector2.up;
        }

        /// <summary>
        /// Returns whether the user is actively giving directional input.
        /// </summary>
        public bool HasInput()
        {
            if (_activeProvider == null) SetupInputProvider();
            return _activeProvider != null && _activeProvider.HasInput();
        }
    }
}
