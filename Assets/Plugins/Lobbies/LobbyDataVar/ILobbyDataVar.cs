using MultiplayerGame.Infrastructure;
using Unity.Services.Lobbies.Models;

namespace MultiplayerGame.Lobbies
{
    public interface ILobbyDataVar
    {
        string Key { get; }
        CallbackValue<DataObject.IndexOptions> Index { get; }
        CallbackValue<DataObject.VisibilityOptions> Visibility { get; }
        void CopyDataFromRemote(DataObject dataObject);
        DataObject ToDataObject();
    }
}