using System;
using MultiplayerGame.Infrastructure;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace MultiplayerGame.Lobbies
{
    public interface ILocalPlayer
    {
        CallbackValue<string> Id { get; }
        CallbackValue<string> ConnectionInfo { get; }
        CallbackValue<string> AllocationId { get; }
        IReadOnlyCallbackValue<bool> IsHost { get; }
        IReadOnlyCallbackValue<DateTime> LastUpdated { get; }
        IReadOnlyCallbackValue<DateTime> Joined { get; }
        IReadOnlyCallbackValue<int> Index { get; }
        void UpdateFromRemoteChanges(LobbyPlayerChanges changes);
        void CopyDataFromRemote(Player player, int index, bool isHost);
        Player ToRemotePlayer();
        void Reset();
    }
}