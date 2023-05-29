namespace MultiplayerGame.Lobbies
{
    public interface IRemoteData
    {
        void Parse(string value);
        string Serialize();
    }
}