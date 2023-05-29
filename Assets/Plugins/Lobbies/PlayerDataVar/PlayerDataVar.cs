using System;
using MultiplayerGame.Infrastructure;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace MultiplayerGame.Lobbies
{
    [Serializable]
    public class PlayerDataVar<TRemoteData> : IPlayerDataVar where TRemoteData : IRemoteData, new()
    {
        [SerializeField] private string _key;
        [SerializeField] private PlayerDataObject.VisibilityOptions _visibility;

        public string Key => _key;
        public PlayerDataObject.VisibilityOptions Visibility => _visibility;
        public TRemoteData Data { get; } = new();

        public PlayerDataVar(string key,
            PlayerDataObject.VisibilityOptions visibility = PlayerDataObject.VisibilityOptions.Public)
        {
            _visibility = visibility;
            _key = key;
        }

        public void CopyDataFromRemote(PlayerDataObject remoteData)
        {
            _visibility = remoteData.Visibility;
            Data.Parse(remoteData.Value);
        }

        public PlayerDataObject ToPlayerDataObject()
        {
            return new PlayerDataObject(_visibility, Data.Serialize());
        }

        public void Reset()
        {
            Data.Parse(default);;
            _visibility = default;
        }
    }
}