using System;
using System.Collections.Generic;
using System.Linq;
using MultiplayerGame.Infrastructure;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace MultiplayerGame.Lobbies
{
    /// <summary>
    /// A local wrapper around a lobby's remote data, with additional functionality for providing that data to UI elements and tracking local player objects.
    /// (The way that the Lobby service handles its data doesn't necessarily match our needs, so we need to map from that to this LocalLobby for use in the sample code.)
    /// </summary>
    [Serializable]
    public abstract class LocalLobbyBase<TLocalPlayer> : MonoBehaviour, ILocalLobby where TLocalPlayer : ILocalPlayer, new()
    {
        public event Action<TLocalPlayer> OnPlayerJoined;
        public event Action<TLocalPlayer> OnPlayerLeft;

        ILocalPlayer ILocalLobby.LocalPlayer => LocalPlayer;

        public LobbyEventCallbacks LobbyEventCallbacks { get; } = new();
        [SerializeField] private TLocalPlayer _localPlayer;
        [SerializeField] private List<TLocalPlayer> _localPlayers = new();
        [SerializeField] private CallbackValue<string> _lobbyName = new("DefaultLobbyName");
        [SerializeField] private CallbackValue<bool> _isLocked = new();
        [SerializeField] private CallbackValue<bool> _isPrivate = new(false);
        [SerializeField] private CallbackValue<int> _maxPlayerCount = new();
        [SerializeField] private CallbackValue<string> _lobbyId = new();
        [SerializeField] private CallbackValue<string> _lobbyCode = new();
        [SerializeField] private CallbackValue<string> _hostId = new();
        [SerializeField] private CallbackValue<int> _availableSlots = new();
        [SerializeField] private CallbackValue<DateTime> _lastUpdated = new();

        public TLocalPlayer LocalPlayer => _localPlayer;
        public IReadOnlyList<TLocalPlayer> LocalPlayers => _localPlayers;
        public CallbackValue<string> LobbyName => _lobbyName;
        public CallbackValue<bool> IsLocked => _isLocked;
        public CallbackValue<bool> IsPrivate => _isPrivate;
        public CallbackValue<int> MaxPlayerCount => _maxPlayerCount;
        public IReadOnlyCallbackValue<string> LobbyId => _lobbyId;
        public IReadOnlyCallbackValue<string> LobbyCode => _lobbyCode;
        public IReadOnlyCallbackValue<string> HostId => _hostId;
        public IReadOnlyCallbackValue<int> AvailableSlots => _availableSlots;
        public IReadOnlyCallbackValue<DateTime> LastUpdated => _lastUpdated;

        public int PlayerCount => _localPlayers.Count;

        private readonly IReadOnlyList<ILobbyDataVar> _dataValues;

        protected LocalLobbyBase()
        {
            _dataValues = LobbyDataFieldsHelper.GetLobbyDataValues(this);
            _lastUpdated.Value = DateTime.Now;
        }

        public void UpdateFromRemoteChanges(ILobbyChanges changes)
        {
            // Lobby Fields
            if (changes.Name.Changed) LobbyName.Value = changes.Name.Value;
            if (changes.IsPrivate.Changed) IsPrivate.Value = changes.IsPrivate.Value;
            if (changes.IsLocked.Changed) IsLocked.Value = changes.IsLocked.Value;
            if (changes.MaxPlayers.Changed) MaxPlayerCount.Value = changes.MaxPlayers.Value;
            if (changes.HostId.Changed) _hostId.Value = changes.HostId.Value;
            if (changes.AvailableSlots.Changed) _availableSlots.Value = changes.AvailableSlots.Value;
            if (changes.LastUpdated.Changed) _lastUpdated.Value = changes.LastUpdated.Value;

            // Custom Lobby Fields
            if (changes.Data.Changed)
            {
                foreach ((string key, var changedValue) in changes.Data.Value)
                {
                    if (changedValue.Removed)
                    {
                        GetLocalValue(key); // TODO Reset();
                    }
                    else if (changedValue.Changed)
                    {
                        GetLocalValue(key).CopyDataFromRemote(changedValue.Value);
                    }
                }
            }

            if (changes.PlayerJoined.Changed)
            {
                foreach (LobbyPlayerJoined lobbyPlayerJoined in changes.PlayerJoined.Value)
                {
                    AddJoinedPlayer(lobbyPlayerJoined);
                }
            }

            if (changes.PlayerLeft.Changed)
            {
                foreach (int leftPlayerIndex in changes.PlayerLeft.Value)
                {
                    RemovePlayer(leftPlayerIndex);
                }
            }

            if (changes.PlayerData.Changed)
            {
                foreach ((int playerIndex, LobbyPlayerChanges playerChanges) in changes.PlayerData.Value)
                {
                    GetLocalPlayer(playerIndex)?.UpdateFromRemoteChanges(playerChanges);
                }
            }
        }

        public void CopyDataFromRemote(Lobby lobby)
        {
            if (lobby == null)
            {
                Debug.LogError("Remote lobby is null, cannot convert.");
                return;
            }

            LobbyName.Value = lobby.Name;
            IsPrivate.Value = lobby.IsPrivate;
            MaxPlayerCount.Value = lobby.MaxPlayers;
            _lobbyId.Value = lobby.Id;
            _hostId.Value = lobby.HostId;
            _lobbyCode.Value = lobby.LobbyCode;
            _availableSlots.Value = lobby.AvailableSlots;
            _lastUpdated.Value = lobby.LastUpdated;

            // Custom Lobby Data Conversions
            foreach (ILobbyDataVar localDataValue in _dataValues)
            {
                if (lobby.Data.TryGetValue(localDataValue.Key, out DataObject dataObject))
                {
                    localDataValue.CopyDataFromRemote(dataObject);
                }
            }

            for (var i = 0; i < lobby.Players.Count; i++)
            {
                Player remotePlayer = lobby.Players[i];
                bool isHost = lobby.HostId.Equals(remotePlayer.Id);

                if (TryGetLocalPlayer(i, out TLocalPlayer localPlayer))
                {
                    localPlayer.CopyDataFromRemote(remotePlayer, i, isHost);
                }
                else
                {
                    localPlayer = LocalPlayer.Id.Value == remotePlayer.Id ? LocalPlayer : new TLocalPlayer();
                    localPlayer.CopyDataFromRemote(remotePlayer, i, isHost);
                    AddPlayer(i, localPlayer);
                }
            }
        }

        public Dictionary<string, DataObject> GetRemoteData()
        {
            var data = _dataValues
                .ToDictionary(value => value.Key, value => value.ToDataObject());

            return data;
        }

        public virtual void Reset()
        {
            _localPlayers.Clear();

            LobbyName.Value = "";
            IsLocked.Value = false;
            IsPrivate.Value = false;
            MaxPlayerCount.Value = 4;
            OnPlayerJoined = null;
            OnPlayerLeft = null;
            _lobbyCode.Value = "";
            _availableSlots.Value = 4;
            _lobbyId.Value = "";
        }

        private ILobbyDataVar GetLocalValue(string key)
        {
            return _dataValues.First(data => data.Key == key);
        }

        private bool TryGetLocalPlayer(int index, out TLocalPlayer player)
        {
            if (PlayerCount > index)
            {
                player = _localPlayers[index];
                return true;
            }

            player = default;
            return false;
        }

        private TLocalPlayer GetLocalPlayer(int index)
        {
            return PlayerCount > index ? _localPlayers[index] : default;
        }

        private void AddPlayer(int index, TLocalPlayer player)
        {
            _localPlayers.Insert(index, player);
            OnPlayerJoined?.Invoke(player);
            Debug.Log($"Added User: {player.Id.Value} to slot {index + 1}/{PlayerCount}");
        }

        private void RemovePlayer(int playerIndex)
        {
            TLocalPlayer player = _localPlayers[playerIndex];

            _localPlayers.Remove(player);
            OnPlayerLeft?.Invoke(player);
        }

        private void AddJoinedPlayer(LobbyPlayerJoined joinedPlayer)
        {
            int index = joinedPlayer.PlayerIndex;
            bool isHost = HostId.Value == joinedPlayer.Player.Id;

            var newPlayer = new TLocalPlayer();
            newPlayer.CopyDataFromRemote(joinedPlayer.Player, index, isHost);
            AddPlayer(index, newPlayer);
        }
    }
}