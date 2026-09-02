using System;
using System.Collections.Generic;
using UnityEngine;

namespace TERRAGRAV.Multiplayer
{
    /// <summary>
    /// Custom room creation and private lobby management.
    /// Handles room codes, player slots, and ready state verification.
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        [Header("Room Properties")]
        [SerializeField] private string _roomCode;
        [SerializeField] private int _maxSlots = 8;

        public string RoomCode => _roomCode;
        public int MaxSlots => _maxSlots;
        public List<string> ConnectedPlayerNames { get; } = new List<string>();

        public event Action<string> OnRoomCreated;
        public event Action<string> OnPlayerJoinedRoom;
        public event Action<string> OnPlayerLeftRoom;

        public void CreatePrivateRoom(int slots = 8)
        {
            _maxSlots = slots;
            _roomCode = GenerateRoomCode();
            ConnectedPlayerNames.Clear();
            Debug.Log($"[RoomManager] Created private room: {_roomCode} with {slots} slots.");
            OnRoomCreated?.Invoke(_roomCode);
        }

        public void JoinPrivateRoom(string code)
        {
            _roomCode = code.ToUpper().Trim();
            Debug.Log($"[RoomManager] Joining private room: {_roomCode}");
            OnPlayerJoinedRoom?.Invoke(_roomCode);
        }

        private string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            char[] codeChars = new char[5];
            for (int i = 0; i < 5; i++)
            {
                codeChars[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
            }
            return new string(codeChars);
        }
    }
}
