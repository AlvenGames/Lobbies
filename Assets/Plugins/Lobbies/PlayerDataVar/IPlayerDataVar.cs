using Unity.Services.Lobbies.Models;

namespace MultiplayerGame.Lobbies
{
    public interface IPlayerDataVar
    {
        string Key { get; }
        PlayerDataObject.VisibilityOptions Visibility { get; }
        void CopyDataFromRemote(PlayerDataObject remoteData);
        PlayerDataObject ToPlayerDataObject();
        void Reset();
    }
}