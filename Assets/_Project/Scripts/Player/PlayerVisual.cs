using UnityEngine;
using TMPro;

namespace TERRAGRAV.Player
{
    /// <summary>
    /// Coordinates the stylized 3D cute cube character visuals, antenna, feet, expressive face,
    /// turning tilt, and floating nametag with colored direction marker.
    /// </summary>
    public class PlayerVisual : MonoBehaviour
    {
        [Header("Mesh Components")]
        [SerializeField] private MeshRenderer _bodyRenderer;
        [SerializeField] private Transform _antennaTransform;
        [SerializeField] private Transform _leftFootTransform;
        [SerializeField] private Transform _rightFootTransform;
        [SerializeField] private Transform _faceTransform;

        [Header("Nametag & Indicator")]
        [SerializeField] private TextMeshPro _nameText;
        [SerializeField] private SpriteRenderer _directionArrow;

        [Header("Animation Tuning")]
        [SerializeField] private float _turnTiltMax = 14f;
        [SerializeField] private float _bobbingSpeed = 12f;
        [SerializeField] private float _bobbingAmount = 0.08f;

        private PlayerController _controller;
        private MaterialPropertyBlock _propBlock;
        private float _walkCycle;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _propBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Applies the player's unique name and saturated faction color.
        /// </summary>
        public void SetupVisual(string playerName, Color playerColor)
        {
            if (_nameText != null)
            {
                _nameText.text = playerName;
                _nameText.color = playerColor;
            }

            if (_directionArrow != null)
            {
                _directionArrow.color = playerColor;
            }

            if (_bodyRenderer != null)
            {
                _bodyRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_BaseColor", playerColor);
                _bodyRenderer.SetPropertyBlock(_propBlock);
            }
        }

        private void Update()
        {
            if (_controller == null || !_controller.Stats.IsAlive) return;

            // 1. Walk Bobbing Animation
            _walkCycle += Time.deltaTime * _bobbingSpeed;
            float bobOffset = Mathf.Sin(_walkCycle) * _bobbingAmount;

            if (_bodyRenderer != null)
            {
                Vector3 pos = _bodyRenderer.transform.localPosition;
                pos.y = 0.5f + bobOffset;
                _bodyRenderer.transform.localPosition = pos;
            }

            // 2. Subtle Foot Alternation
            if (_leftFootTransform != null && _rightFootTransform != null)
            {
                float footBob = Mathf.Sin(_walkCycle) * 0.05f;
                _leftFootTransform.localPosition = new Vector3(-0.25f, -0.4f + footBob, 0f);
                _rightFootTransform.localPosition = new Vector3(0.25f, -0.4f - footBob, 0f);
            }

            // 3. Keep Nametag Facing Camera (60 degree pitch)
            if (_nameText != null)
            {
                _nameText.transform.rotation = Quaternion.Euler(60f, 0f, 0f);
            }
        }
    }
}
