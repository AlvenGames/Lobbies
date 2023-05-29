using System;

namespace MultiplayerGame.Lobbies
{
    [Serializable]
    public class RemoteString : RemoteCallbackValue<string>
    {
        protected override string GetParsed(string value)
        {
            return value;
        }

        protected override string GetSerialized(string value)
        {
            return value;
        }
    }
}