using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MultiplayerGame.Lobbies
{
    internal static class LobbyDataFieldsHelper
    {
        private static readonly Dictionary<Type, FieldInfo[]> FieldTypes = new();

        private static FieldInfo[] GetFieldsForType<T>(Type type, List<FieldInfo> list = null)
        {
            if (list == null)
            {
                list = new List<FieldInfo>();
                list.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => typeof(T).IsAssignableFrom(f.FieldType)));
            }
            else
            {
                list.AddRange(type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(f => typeof(T).IsAssignableFrom(f.FieldType)));
            }

            return type.BaseType == null ? list.ToArray() : GetFieldsForType<T>(type.BaseType, list);
        }

        private static FieldInfo[] GetFieldInfoForType<T>(Type type)
        {
            if (FieldTypes.All(keyValuePair => keyValuePair.Key != type))
            {
                FieldTypes.Add(type, GetFieldsForType<T>(type));
            }

            return FieldTypes[type];
        }

        public static IReadOnlyList<IPlayerDataVar> GetPlayerDataValues(object obj)
        {
            return GetFieldInfoForType<IPlayerDataVar>(obj.GetType()).Select(f => (IPlayerDataVar)f.GetValue(obj)).ToArray();
        }

        public static IReadOnlyList<ILobbyDataVar> GetLobbyDataValues(object obj)
        {
            return GetFieldInfoForType<ILobbyDataVar>(obj.GetType()).Select(f => (ILobbyDataVar)f.GetValue(obj)).ToArray();
        }
    }
}