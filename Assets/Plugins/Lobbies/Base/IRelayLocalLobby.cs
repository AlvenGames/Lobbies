namespace MultiplayerGame.Lobbies
{
    public interface IRelayLocalLobby : ILocalLobby
    {
        LobbyDataVar<RemoteString> RelayJoinCode { get; }
    }
}