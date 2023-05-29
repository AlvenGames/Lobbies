using System;
using System.Collections.Generic;
using MultiplayerGame.Infrastructure;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace MultiplayerGame.Lobbies
{
    public interface ILocalLobby
    {
        ILocalPlayer LocalPlayer { get; }
        LobbyEventCallbacks LobbyEventCallbacks { get; }
        CallbackValue<string> LobbyName { get; }
        CallbackValue<bool> IsLocked { get; }
        CallbackValue<bool> IsPrivate { get; }
        CallbackValue<int> MaxPlayerCount { get; }
        IReadOnlyCallbackValue<int> AvailableSlots { get; }
        IReadOnlyCallbackValue<DateTime> LastUpdated { get; }
        IReadOnlyCallbackValue<string> LobbyId { get; }
        IReadOnlyCallbackValue<string> LobbyCode { get; }
        IReadOnlyCallbackValue<string> HostId { get; }
        void UpdateFromRemoteChanges(ILobbyChanges changes);
        void CopyDataFromRemote(Lobby lobby);
        Dictionary<string, DataObject> GetRemoteData();
    }
}