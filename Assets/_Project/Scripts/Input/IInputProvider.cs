using UnityEngine;

namespace TERRAGRAV.Input
{
    /// <summary>
    /// Contract for all input providers (Standalone PC, Mobile Virtual Joystick, Gamepad).
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// Returns the normalized 2D movement direction vector.
        /// </summary>
        Vector2 GetMovementDirection();

        /// <summary>
        /// Returns true if an active input is currently being held or pressed.
        /// </summary>
        bool HasInput();
    }
}
