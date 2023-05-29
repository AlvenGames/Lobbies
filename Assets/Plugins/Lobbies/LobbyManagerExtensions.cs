using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace MultiplayerGame.Lobbies
{
    public static class LobbyManagerExtensions
    {
        public static async Task<Lobby> UpdateLobbyDataAsync(this LobbyManager lobbyManager, string lobbyId, 
            ILobbyDataVar lobbyDataVar)
        {
            return await lobbyManager.UpdateLobbyDataAsync(lobbyId, lobbyDataVar.ToUpdateLobbyOptions());
        }

        public static async Task<Lobby> UpdateLobbyDataAsync(this LobbyManager lobbyManager, ILocalLobby localLobby, 
            ILobbyDataVar lobbyDataVar)
        {
            return await lobbyManager.UpdateLobbyDataAsync(localLobby.LobbyId.Value, lobbyDataVar);
        }

        public static async Task<Lobby> UpdateLobbyDataAsync<T>(this LobbyManager lobbyManager, ILocalLobby localLobby,
            LobbyDataVar<T> lobbyDataVar) where T : IRemoteData, new()
        {
            return await lobbyManager.UpdateLobbyDataAsync(localLobby.LobbyId.Value, lobbyDataVar);
        }

        public static async Task<Lobby> CreateAndBindLobbyAsync(this LobbyManager lobbyManager, ILocalLobby localLobby)
        {
            Lobby lobby = await lobbyManager.CreateLobbyAsync(
                localLobby.LobbyName.Value,
                localLobby.MaxPlayerCount.Value,
                new CreateLobbyOptions
                {
                    Data = localLobby.GetRemoteData(),
                    IsPrivate = localLobby.IsPrivate.Value,
                    Player = localLobby.LocalPlayer.ToRemotePlayer()
                });
            await lobbyManager.BindLocalLobbyToRemote(localLobby, lobby);
            return lobby;
        }

        public static async Task<Lobby> QuickJoinAndBindLobbyAsync(this LobbyManager lobbyManager, 
            ILocalLobby localLobby)
        {
            Lobby lobby = await lobbyManager.QuickJoinLobbyAsync(new QuickJoinLobbyOptions
            {
                Player = localLobby.LocalPlayer.ToRemotePlayer()
            });
            await lobbyManager.BindLocalLobbyToRemote(localLobby, lobby);
            return lobby;
        }
    }
}