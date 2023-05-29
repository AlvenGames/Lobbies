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
    /// Data for a local player instance. This will update data and is observed to know when to push local player changes to the entire lobby.
    /// </summary>
    [Serializable]
    public abstract class LocalPlayerBase : MonoBehaviour, ILocalPlayer
    {
        [SerializeField] private CallbackValue<string> _id = new();
        [SerializeField] private CallbackValue<string> _connectionInfo = new();
        [SerializeField] private CallbackValue<string> _allocationId = new();
        [SerializeField] private CallbackValue<bool> _isHost = new(false);
        [SerializeField] private CallbackValue<DateTime> _lastUpdated = new();
        [SerializeField] private CallbackValue<DateTime> _joined = new();
        [SerializeField] private CallbackValue<int> _index = new(0);

        public CallbackValue<string> Id => _id;
        public CallbackValue<string> ConnectionInfo => _connectionInfo;
        public CallbackValue<string> AllocationId => _allocationId;
        public IReadOnlyCallbackValue<bool> IsHost => _isHost;
        public IReadOnlyCallbackValue<DateTime> LastUpdated => _lastUpdated;
        public IReadOnlyCallbackValue<DateTime> Joined => _joined;
        public IReadOnlyCallbackValue<int> Index => _index;

        private readonly IReadOnlyDictionary<string, IPlayerDataVar> _dataVars;

        protected LocalPlayerBase(Player remotePlayer, int index, bool isHost) : this()
        {
            CopyDataFromRemote(remotePlayer, index, isHost);
        }

        protected LocalPlayerBase()
        {
            _dataVars = LobbyDataFieldsHelper.GetPlayerDataValues(this).ToDictionary(p => p.Key);
        }

        public void UpdateFromRemoteChanges(LobbyPlayerChanges changes)
        {
            if (changes.ConnectionInfoChanged.Changed)
            {
                ConnectionInfo.Value = changes.ConnectionInfoChanged.Value;
                Debug.Log($"ConnectionInfo for player {changes.PlayerIndex} changed to {ConnectionInfo.Value}");
            }

            if (changes.LastUpdatedChanged.Changed)
            {
                _lastUpdated.Value = changes.LastUpdatedChanged.Value;
            }

            // There are changes on the Player
            if (!changes.ChangedData.Changed) return;

            foreach ((string changedKey, var changedValue) in changes.ChangedData.Value)
            {
                // There are changes on some of the changes in the player list of changes
                if (changedValue.Changed)
                {
                    _dataVars[changedKey].CopyDataFromRemote(changedValue.Value);
                }
                else if (changedValue.Removed)
                {
                    _dataVars[changedKey].Reset();
                }
            }
        }

        public void CopyDataFromRemote(Player player, int index, bool isHost)
        {
            Id.Value = player.Id;
            ConnectionInfo.Value = player.ConnectionInfo;
            AllocationId.Value = player.AllocationId;

            _lastUpdated.Value = player.LastUpdated;
            _joined.Value = player.Joined;
            _index.Value = index;
            _isHost.Value = isHost;

            foreach ((string key, IPlayerDataVar playerDataValue) in _dataVars)
            {
                if (player.Data.TryGetValue(key, out PlayerDataObject playerDataObject))
                {
                    playerDataValue.CopyDataFromRemote(playerDataObject);
                }
            }
        }

        public Player ToRemotePlayer()
        {
            var data = new Dictionary<string, PlayerDataObject>();
            foreach ((string key, IPlayerDataVar playerDataValue) in _dataVars)
            {
                data.Add(key, playerDataValue.ToPlayerDataObject());
            }

            return new Player(Id.Value, ConnectionInfo.Value, data, AllocationId.Value, Joined.Value,
                LastUpdated.Value);
        }

        public virtual void Reset()
        {
            _isHost.Value = false;
        }
    }
}