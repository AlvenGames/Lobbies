using System;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace MultiplayerGame.Lobbies
{
    /// <summary>
    /// An abstraction layer between the direct calls into the Lobby API and the outcomes you actually want. E.g. you can request to get a readable list of
    /// current lobbies and not need to make the query call directly.
    /// </summary>
    /// Manages one Lobby at a time, Only entry points to a lobby with ID is via JoinAsync, CreateAsync, and QuickJoinAsync
    public class LobbyManager : IDisposable
    {
        // Rate Limits are posted here: https://docs.unity.com/lobby/rate-limits.html
        public readonly ServiceRateLimiter QueryCooldown = new(1, 1f);
        public readonly ServiceRateLimiter CreateCooldown = new(2, 6f);
        public readonly ServiceRateLimiter JoinCooldown = new(2, 6f);
        public readonly ServiceRateLimiter QuickJoinCooldown = new(1, 10f);
        public readonly ServiceRateLimiter GetLobbyCooldown = new(1, 1f);
        public readonly ServiceRateLimiter DeleteLobbyCooldown = new(2, 1f);
        public readonly ServiceRateLimiter UpdateLobbyCooldown = new(5, 5f);
        public readonly ServiceRateLimiter UpdatePlayerCooldown = new(5, 5f);
        public readonly ServiceRateLimiter LeaveLobbyOrRemovePlayer = new(5, 1);
        public readonly ServiceRateLimiter HeartbeatCooldown = new(5, 30);

        // Once connected to a lobby, cache the local lobby object so we don't query for it for every lobby operation.
        // (This assumes that the game will be actively in just one lobby at a time, though they could be in more on the service side.)
        public readonly LobbyHeartbeat LobbyHeartbeat;

        public LobbyManager()
        {
            LobbyHeartbeat = new LobbyHeartbeat(HeartbeatCooldown);
        }

        public async Task BindLocalLobbyToRemote(ILocalLobby localLobby, Lobby remoteLobby)
        {
            var callbacks = new LobbyEventCallbacks();

            // LobbyService.Instance.CreateOrJoinLobbyAsync()

            localLobby.CopyDataFromRemote(remoteLobby);
            callbacks.LobbyChanged += async changes =>
            {
                if (changes.LobbyDeleted)
                {
                    await LeaveLobbyAsync(remoteLobby.Id, localLobby.LocalPlayer.Id.Value);
                    return;
                }

                Debug.Log($"Lobby Changed");
                localLobby.UpdateFromRemoteChanges(changes);
            };

            callbacks.LobbyEventConnectionStateChanged += lobbyEventConnectionState =>
            {
                Debug.Log($"Lobby ConnectionState Changed to {lobbyEventConnectionState}");
            };

            callbacks.KickedFromLobby += () =>
            {
                Debug.Log("Left Lobby");
                callbacks = null;
                Dispose();
            };

            await LobbyService.Instance.SubscribeToLobbyEventsAsync(remoteLobby.Id, callbacks);
        }

        public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, CreateLobbyOptions options)
        {
            var createTask = LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            return await CreateCooldown.RunUntilCooldownT(createTask);
        }

        public async Task<Lobby> JoinLobbyByIdAsync(string lobbyId, JoinLobbyByIdOptions options = default)
        {
            var joinTask = LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            return await JoinCooldown.RunUntilCooldownT(joinTask);
        }

        public async Task<Lobby> JoinLobbyByCodeAsync(string lobbyCode, JoinLobbyByCodeOptions options = default)
        {
            var joinTask = LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            return await JoinCooldown.RunUntilCooldownT(joinTask);
        }

        public async Task<Lobby> QuickJoinLobbyAsync(QuickJoinLobbyOptions options = default)
        {
            var quickJoinTask = LobbyService.Instance.QuickJoinLobbyAsync(options);
            return await QuickJoinCooldown.RunUntilCooldownT(quickJoinTask);
        }

        public async Task<QueryResponse> QueryLobbiesAsync(QueryLobbiesOptions options = default)
        {
            var queryTask = LobbyService.Instance.QueryLobbiesAsync(options);
            return await QueryCooldown.RunUntilCooldownT(queryTask);
        }

        public async Task<Lobby> GetLobbyAsync(string lobbyId)
        {
            var getTask = LobbyService.Instance.GetLobbyAsync(lobbyId);
            return await GetLobbyCooldown.RunUntilCooldownT(getTask);
        }

        public async Task LeaveLobbyAsync(string lobbyId, string playerId)
        {
            Task removeTask = LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
            await LeaveLobbyOrRemovePlayer.RunUntilCooldown(removeTask);
            Dispose();
        }

        public async Task<Lobby> UpdatePlayerAsync(string lobbyId, string playerId, UpdatePlayerOptions options)
        {
            var updateTask = LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, options);
            return await UpdatePlayerCooldown.RunUntilCooldownT(updateTask);
        }

        public async Task<Lobby> UpdatePlayerRelayInfoAsync(string lobbyId, string playerId, string allocationId, string connectionInfo)
        {
            var options = new UpdatePlayerOptions
            {
                AllocationId = allocationId, ConnectionInfo = connectionInfo
            };
            return await UpdatePlayerAsync(lobbyId, playerId, options);
        }

        public async Task<Lobby> UpdateLobbyDataAsync(string lobbyId, UpdateLobbyOptions options)
        {
            var updateTask = LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
            return await UpdateLobbyCooldown.RunUntilCooldownT(updateTask);
        }

        public async Task DeleteLobbyAsync(string lobbyId)
        {
            Task deleteTask = LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            await DeleteLobbyCooldown.RunUntilCooldown(deleteTask);
        }

        public void Dispose()
        {
            LobbyHeartbeat.EndTracking();
        }
    }
}