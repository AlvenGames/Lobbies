using System;
using MultiplayerGame.Infrastructure;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace MultiplayerGame.Lobbies
{
    [Serializable]
    public class LobbyDataVar<TRemoteData> : ILobbyDataVar where TRemoteData : IRemoteData, new()
    {
        [SerializeField] private TRemoteData _data = new();
        [SerializeField] private string _key;
        [SerializeField] private CallbackValue<DataObject.IndexOptions> _index;
        [SerializeField] private CallbackValue<DataObject.VisibilityOptions> _visibility;
        public string Key => _key;
        public CallbackValue<DataObject.IndexOptions> Index => _index;
        public CallbackValue<DataObject.VisibilityOptions> Visibility => _visibility;
        public TRemoteData Data => _data;

        public LobbyDataVar(string key, DataObject.VisibilityOptions visibility,
            DataObject.IndexOptions index = default)
        {
            _key = key;
            _visibility = new CallbackValue<DataObject.VisibilityOptions>(visibility);
            _index = new CallbackValue<DataObject.IndexOptions>(index);
        }

        public void CopyDataFromRemote(DataObject remoteData)
        {
            Visibility.Value = remoteData.Visibility;
            Data.Parse(remoteData.Value);
            Index.Value = remoteData.Index;
        }

        public DataObject ToDataObject()
        {
            return new DataObject(Visibility.Value, Data.Serialize(), Index.Value);
        }
    }
}