using System;
using UnityEngine;

namespace Script.CoreLib
{
    public class NotifyValue<T>
    {
        private T _value;
        
        public event Action OnValueChanged;
        
        public T Value
        {
            get => _value;

            set
            {
                T before = _value;
                _value = value;
                if (before == null && _value != null || before.Equals(value))
                {
                    OnValueChanged?.Invoke();
                }
            }
        }

        public NotifyValue()
        {
            _value = default(T);
        }
        
        public NotifyValue(T value)
        {
            _value = value;
        }
    }
}