using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TERRAGRAV.Core;

namespace TERRAGRAV.UI
{
    /// <summary>
    /// Master 2.5D Mobile Game HUD.
    /// Manages TopBar (Menu + 48ms ping), match timer capsule, 8-player leaderboard,
    /// bottom-center "MY TERRITORY" panel, and lightning boost action controls.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Top Bar Elements")]
        [SerializeField] private TextMeshProUGUI _pingText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Button _menuButton;

        [Header("Leaderboard Panel (8 Players)")]
        [SerializeField] private Transform _leaderboardContainer;
        [SerializeField] private GameObject _leaderboardRowPrefab;

        [Header("Bottom Territory Panel")]
        [SerializeField] private Image _myColorSwatch;
        [SerializeField] private TextMeshProUGUI _myTerritoryPercentageText;

        [Header("Action Controls")]
        [SerializeField] private Button _boostActionButton;
        [SerializeField] private Image _boostEnergyRingFill;
        [SerializeField] private Button _shieldActionButton;

        private void OnEnable()
        {
            GameEvents.OnMatchTimerUpdated += UpdateTimer;
            GameEvents.OnTerritoryChanged += UpdateLocalTerritory;
        }

        private void OnDisable()
        {
            GameEvents.OnMatchTimerUpdated -= UpdateTimer;
            GameEvents.OnTerritoryChanged -= UpdateLocalTerritory;
        }

        private void Start()
        {
            if (_pingText != null) _pingText.text = "48ms";
            if (_myTerritoryPercentageText != null) _myTerritoryPercentageText.text = "31.4%";
        }

        public void UpdateTimer(float remainingSeconds)
        {
            if (_timerText == null) return;
            int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }

        public void UpdateLocalTerritory(int playerId, int cellCount, float percentage)
        {
            if (playerId == 1 && _myTerritoryPercentageText != null)
            {
                _myTerritoryPercentageText.text = $"{percentage:0.0}%";
            }
        }

        public void UpdateBoostEnergy(float current, float max)
        {
            if (_boostEnergyRingFill != null && max > 0)
            {
                _boostEnergyRingFill.fillAmount = Mathf.Clamp01(current / max);
            }
        }
    }
}
