using UnityEngine;

namespace TERRAGRAV.Input
{
    /// <summary>
    /// Adapts the UI VirtualJoystick into the clean IInputProvider abstraction.
    /// Decouples PlayerMovement from any direct UI dependencies.
    /// </summary>
    public class MobileInputProvider : IInputProvider
    {
        private readonly VirtualJoystick _joystick;
        private Vector2 _cachedDirection = Vector2.up;

        public MobileInputProvider(VirtualJoystick joystick)
        {
            _joystick = joystick;
        }

        public Vector2 GetMovementDirection()
        {
            if (_joystick != null && _joystick.IsHeld)
            {
                Vector2 dir = _joystick.InputDirection;
                if (dir.sqrMagnitude > 0.001f)
                {
                    _cachedDirection = dir.normalized;
                    return _cachedDirection;
                }
            }

            // Fallback for simple direct touch swipe when joystick is not held
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved && touch.deltaPosition.sqrMagnitude > 1f)
                {
                    _cachedDirection = touch.deltaPosition.normalized;
                    return _cachedDirection;
                }
            }

            return _cachedDirection;
        }

        public bool HasInput()
        {
            return (_joystick != null && _joystick.IsHeld) || UnityEngine.Input.touchCount > 0;
        }
    }
}
