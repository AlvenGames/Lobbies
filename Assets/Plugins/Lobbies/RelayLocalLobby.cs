using System;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace MultiplayerGame.Lobbies
{
    [Serializable]
    public class RelayLocalLobby : LocalLobbyBase<LocalPlayer>, IRelayLocalLobby
    {
        [SerializeField] private LobbyDataVar<RemoteString> _relayJoinCode = new(nameof(RelayJoinCode), DataObject.VisibilityOptions.Member);
        public LobbyDataVar<RemoteString> RelayJoinCode => _relayJoinCode;

        // public RelayLocalLobby() : base(new LocalPlayer()) { }
    }
}