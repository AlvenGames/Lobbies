namespace MultiplayerGame.Lobbies
{
    public static class LocalLobbyExtensions
    {
        public static void Configure(this ILocalLobby localLobby, string lobbyName, int maxPlayersCount, bool? isPrivate)
        {
            localLobby.LobbyName.Value = lobbyName;
            localLobby.MaxPlayerCount.Value = maxPlayersCount;
            if (isPrivate != null)
            {
                localLobby.IsPrivate.Value = isPrivate.Value;
            }
        }
    }
}