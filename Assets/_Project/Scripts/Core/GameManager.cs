using System.Collections;
using UnityEngine;

namespace TERRAGRAV.Core
{
    /// <summary>
    /// Master lifecycle manager controlling game progression:
    /// Boot -> Lobby -> Countdown -> Playing -> GameOver.
    /// Manages match timer and notifies subscribers via GameEvents.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Match Settings")]
        [Tooltip("Total duration of the match in seconds. Default: 480s (8 minutes).")]
        [SerializeField] private float _matchDuration = 480f;

        [Tooltip("Countdown duration in seconds prior to match start.")]
        [SerializeField] private float _countdownDuration = 3f;

        [Tooltip("Automatically progress from Boot through to Playing on Awake/Start.")]
        [SerializeField] private bool _autoStartSequence = true;

        [Header("Runtime State")]
        [SerializeField] private GameState _currentState = GameState.Boot;

        private float _timeRemaining;
        private Coroutine _lifecycleCoroutine;

        public GameState CurrentState => _currentState;
        public float TimeRemaining => _timeRemaining;
        public float MatchDuration => _matchDuration;
        public bool IsPlaying => _currentState == GameState.Playing;

        private void Awake()
        {
            ServiceLocator.Register<GameManager>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<GameManager>();
        }

        private void Start()
        {
            if (_autoStartSequence)
            {
                StartLifecycle();
            }
        }

        /// <summary>
        /// Starts the full game lifecycle sequence.
        /// </summary>
        public void StartLifecycle()
        {
            if (_lifecycleCoroutine != null)
            {
                StopCoroutine(_lifecycleCoroutine);
            }
            _lifecycleCoroutine = StartCoroutine(GameLifecycleRoutine());
        }

        private IEnumerator GameLifecycleRoutine()
        {
            // 1. Boot Phase
            SetState(GameState.Boot);
            yield return null;

            // 2. Lobby Phase
            SetState(GameState.Lobby);
            yield return new WaitForSeconds(0.5f);

            // 3. Countdown Phase (3, 2, 1, GO)
            SetState(GameState.Countdown);
            int count = Mathf.CeilToInt(_countdownDuration);
            while (count > 0)
            {
                GameEvents.TriggerCountdownTick(count);
                yield return new WaitForSeconds(1f);
                count--;
            }
            GameEvents.TriggerCountdownTick(0); // 0 = "GO!"

            // 4. Playing Phase
            SetState(GameState.Playing);
            _timeRemaining = _matchDuration;

            while (_timeRemaining > 0f)
            {
                _timeRemaining -= Time.deltaTime;
                GameEvents.TriggerMatchTimerUpdated(_timeRemaining);
                yield return null;
            }

            _timeRemaining = 0f;
            GameEvents.TriggerMatchTimerUpdated(0f);

            // 5. GameOver Phase
            SetState(GameState.GameOver);
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            Debug.Log($"[GameManager] Lifecycle Transition -> {newState}");
            GameEvents.TriggerGameStateChanged(newState);
        }

        public void RestartGame()
        {
            StartLifecycle();
        }
    }
}
