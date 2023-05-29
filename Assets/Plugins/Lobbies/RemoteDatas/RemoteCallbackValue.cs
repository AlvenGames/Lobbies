using System;
using MultiplayerGame.Infrastructure;

namespace MultiplayerGame.Lobbies
{
    [Serializable]
    public abstract class RemoteCallbackValue<T> : CallbackValue<T>, IRemoteData
    {
        protected abstract T GetParsed(string value);
        protected abstract string GetSerialized(T value);

        public void Parse(string value)
        {
            Value = GetParsed(value);
        }

        public string Serialize()
        {
            return GetSerialized(Value);
        }
    }
}