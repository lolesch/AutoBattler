using System;
using NaughtyAttributes;
using UnityEngine;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public sealed class MutableInt : IMutable<int>
    {
        [SerializeField, ReadOnly] private int totalValue;
        [SerializeField, ReadOnly] private MutableInt mutableFloat;
        
        public MutableInt( int baseValue )
        {
            mutableFloat = new MutableInt( baseValue );
            totalValue = baseValue;
            OnTotalChanged = null;
        }
        
        public static implicit operator int( MutableInt mutableInt ) => mutableInt!.totalValue;
        
        public event Action<int> OnTotalChanged;

        public void AddModifier( Modifier modifier )
        {
            mutableFloat.AddModifier( modifier );
            CalculateTotalValue();
        }

        public bool TryRemoveModifier( Modifier modifier )
        {
            if( mutableFloat.TryRemoveModifier( modifier ) )
            {
                CalculateTotalValue();
                return true;
            }
            
            return false;
        }

        private void CalculateTotalValue()
        {
            var newTotal = Mathf.RoundToInt( mutableFloat );
            //newTotal = Mathf.Clamp(newTotal, range.min, range.max);

            if( Mathf.Approximately( totalValue, newTotal ) )
                return;

            totalValue = newTotal;
            OnTotalChanged?.Invoke( totalValue );
        }
    }
}