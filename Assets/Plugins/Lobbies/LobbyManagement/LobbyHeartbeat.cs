using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace MultiplayerGame.Lobbies
{
    public class LobbyHeartbeat
    {
        public Lobby CurrentLobby { get; private set; }
        public readonly ServiceRateLimiter HeartbeatCooldown;
        private CancellationTokenSource _cts;

        public bool IsTracking => CurrentLobby != null;

        public LobbyHeartbeat(ServiceRateLimiter heartbeatCooldown)
        {
            HeartbeatCooldown = heartbeatCooldown;
        }

        public void BeginTracking(Lobby lobby)
        {
            EndTracking();
            CurrentLobby = lobby;
            _cts = new CancellationTokenSource();
            HeartbeatLoop(_cts.Token);
        }

        public void EndTracking()
        {
            CurrentLobby = null;
            _cts?.Cancel();
            _cts?.Dispose();
        }

        // Since the LobbyManager maintains the "connection" to the lobby, we will continue to heartbeat until host leaves.
        private async void HeartbeatLoop(CancellationToken cancellationToken)
        {
            while (CurrentLobby != null && !cancellationToken.IsCancellationRequested)
            {
                Task heartbeatTask = LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
                await HeartbeatCooldown.RunUntilCooldown(heartbeatTask);
                Debug.Log("Heartbeat");

                await Task.Delay(8000, cancellationToken);
            }
        }
    }
}