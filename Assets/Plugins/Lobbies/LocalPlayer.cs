using System;
using Unity.Services.Lobbies.Models;

namespace MultiplayerGame.Lobbies
{
    [Serializable]
    public class LocalPlayer : LocalPlayerBase
    {
        public PlayerDataVar<RemoteString> DisplayName = new(nameof(DisplayName), PlayerDataObject.VisibilityOptions.Member);
    }
}