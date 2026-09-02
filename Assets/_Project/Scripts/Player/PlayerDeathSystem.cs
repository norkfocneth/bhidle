using System.Collections;
using UnityEngine;
using TERRAGRAV.Core;
using TERRAGRAV.Territory;
using TERRAGRAV.VFX;

namespace TERRAGRAV.Player
{
    /// <summary>
    /// Master death and elimination controller.
    /// Handles death sequence, territory clearing, confetti explosions, camera feedback, and scoring.
    /// </summary>
    public class PlayerDeathSystem : MonoBehaviour
    {
        [Header("VFX Prefab Reference")]
        [SerializeField] private ConfettiExplosionVFX _explosionVFXPrefab;

        private PlayerController _controller;
        private TerritoryGrid _grid;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out TerritoryGrid grid))
            {
                _grid = grid;
            }
        }

        /// <summary>
        /// Executes the complete elimination sequence for this player entity.
        /// </summary>
        public void Eliminate(int killerPlayerId)
        {
            if (_controller == null || !_controller.Stats.IsAlive) return;

            Vector3 deathPos = transform.position;
            int victimId = _controller.Stats.PlayerId;
            Color playerColor = _controller.Settings != null ? _controller.Settings.CaptureColor : Color.cyan;

            // 1. Mark dead and halt locomotion
            _controller.Stats.SetAlive(false);
            _controller.Movement.SetCanMove(false);

            // 2. Clear trail
            if (_controller.Trail != null)
            {
                _controller.Trail.ClearTrail();
            }

            // 3. Wipe all claimed territory of the victim from the grid
            WipePlayerTerritory(victimId);

            // 4. Spawn Confetti Particle Burst
            SpawnExplosionVFX(deathPos, playerColor);

            // 5. Trigger Camera Feedback if local player was involved
            if (_controller.Stats.IsLocalPlayer || killerPlayerId == 1)
            {
                GameEvents.TriggerCameraShake(0.6f, 0.35f);
            }

            // 6. Dispatch Global Elimination Event
            GameEvents.TriggerPlayerEliminated(killerPlayerId, victimId, deathPos);

            // 7. Hide visual mesh
            _controller.Eliminate();
        }

        private void WipePlayerTerritory(int playerId)
        {
            if (_grid == null)
            {
                ServiceLocator.TryGet(out _grid);
                if (_grid == null) return;
            }

            int width = _grid.Width;
            int height = _grid.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TerritoryCell cell = _grid.GetCell(x, y);
                    if (cell.ownerId == playerId)
                    {
                        _grid.SetCell(x, y, TerritoryCell.CreateEmpty());
                    }
                }
            }

            _grid.NotifyRegionModified(0, 0, width - 1, height - 1);
        }

        private void SpawnExplosionVFX(Vector3 position, Color color)
        {
            if (_explosionVFXPrefab != null)
            {
                ConfettiExplosionVFX vfx = Instantiate(_explosionVFXPrefab, position, Quaternion.identity);
                vfx.PlayBurst(position, color);
                Destroy(vfx.gameObject, 2.5f);
            }
            else
            {
                // Procedural runtime fallback
                GameObject vfxObj = new GameObject("ConfettiBurst_Runtime");
                ConfettiExplosionVFX vfx = vfxObj.AddComponent<ConfettiExplosionVFX>();
                vfx.PlayBurst(position, color);
                Destroy(vfxObj, 2.5f);
            }
        }
    }
}
