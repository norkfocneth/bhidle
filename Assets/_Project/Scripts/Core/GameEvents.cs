using System;
using System.Collections.Generic;
using UnityEngine;
using TERRAGRAV.Trail;

namespace TERRAGRAV.Core
{
    /// <summary>
    /// Global decoupled event bus for game events to prevent tight coupling between systems.
    /// Does not maintain unnecessary mutable global state.
    /// </summary>
    public static class GameEvents
    {
        // Lifecycle & Timer Events
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<float> OnMatchTimerUpdated;
        public static event Action<int> OnCountdownTick;

        // Player & Gameplay Metric Events
        public static event Action<int, int, float> OnTerritoryChanged; // playerId, cellCount, percentage
        public static event Action<int, int> OnScoreChanged;           // playerId, score
        public static event Action<int, Vector3> OnPlayerDeath;         // playerId, deathPosition
        public static event Action<int> OnPlayerSpawned;               // playerId
        public static event Action<int, int> OnPlayerCapturedTerritory; // playerId, cellCountCaptured

        // Combat & Elimination Events
        public static event Action<int, int, Vector3> OnPlayerEliminated; // killerId, victimId, position
        public static event Action<float, float> OnCameraShake;           // intensity, duration

        // Trail Lifecycle Events
        public static event Action<int> OnTrailStarted;
        public static event Action<int, TrailPoint> OnTrailPointAdded;
        public static event Action<int, IReadOnlyList<TrailPoint>> OnTrailClosed;
        public static event Action<int> OnTrailCleared;

        // Dispatchers
        public static void TriggerGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
        public static void TriggerMatchTimerUpdated(float remainingTime) => OnMatchTimerUpdated?.Invoke(remainingTime);
        public static void TriggerCountdownTick(int count) => OnCountdownTick?.Invoke(count);
        public static void TriggerTerritoryChanged(int playerId, int cellCount, float percentage) => OnTerritoryChanged?.Invoke(playerId, cellCount, percentage);
        public static void TriggerScoreChanged(int playerId, int score) => OnScoreChanged?.Invoke(playerId, score);
        public static void TriggerPlayerDeath(int playerId, Vector3 position) => OnPlayerDeath?.Invoke(playerId, position);
        public static void TriggerPlayerSpawned(int playerId) => OnPlayerSpawned?.Invoke(playerId);
        public static void TriggerPlayerCapturedTerritory(int playerId, int count) => OnPlayerCapturedTerritory?.Invoke(playerId, count);

        public static void TriggerPlayerEliminated(int killerId, int victimId, Vector3 pos) => OnPlayerEliminated?.Invoke(killerId, victimId, pos);
        public static void TriggerCameraShake(float intensity, float duration) => OnCameraShake?.Invoke(intensity, duration);

        public static void TriggerTrailStarted(int playerId) => OnTrailStarted?.Invoke(playerId);
        public static void TriggerTrailPointAdded(int playerId, TrailPoint point) => OnTrailPointAdded?.Invoke(playerId, point);
        public static void TriggerTrailClosed(int playerId, IReadOnlyList<TrailPoint> points) => OnTrailClosed?.Invoke(playerId, points);
        public static void TriggerTrailCleared(int playerId) => OnTrailCleared?.Invoke(playerId);
    }
}
