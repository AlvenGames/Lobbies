#nullable enable
using System;
using UnityEngine;

namespace MultiplayerGame.Infrastructure
{
    public interface IReadOnlyCallbackValue<out T>
    {
        event Action<T?> OnChanged;
        T? Value { get; }
    }

    [Serializable]
    public class CallbackValue<T> : IReadOnlyCallbackValue<T>
    {
        public event Action<T?>? OnChanged;
        [SerializeField] protected T? _value;

        public T? Value
        {
            get => _value;
            set
            {
                if (_value != null && _value.Equals(value))
                    return;
                _value = value;
                OnChanged?.Invoke(_value);
            }
        }

        public CallbackValue()
        {
        }

        public CallbackValue(T value)
        {
            _value = value;
        }

        public void ForceSet(T value)
        {
            _value = value;
            OnChanged?.Invoke(_value);
        }

        public void SetNoCallback(T value)
        {
            _value = value;
        }

        public static explicit operator T?(CallbackValue<T> callbackValue)
        {
            return callbackValue.Value;
        }

        public static implicit operator CallbackValue<T>(T value)
        {
            return new CallbackValue<T>(value);
        }
    }
}
