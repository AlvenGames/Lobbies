using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace MultiplayerGame.Lobbies
{
    public static class LobbyDataVarExtensions
    {
        public static UpdateLobbyOptions ToUpdateLobbyOptions(this ILobbyDataVar dataVar)
        {
            return new UpdateLobbyOptions
            {
                Data = dataVar.ToDataObjectDictionary()
            };
        }

        private static Dictionary<string, DataObject> ToDataObjectDictionary(this ILobbyDataVar dataVar)
        {
            return new Dictionary<string, DataObject>
            {
                { dataVar.Key, dataVar.ToDataObject() }
            };
        }
    }
}