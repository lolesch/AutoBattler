using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public sealed class MutableFloat : IMutable<float>, IFormattable
    {
        [SerializeField, ReadOnly] private float totalValue;
        [SerializeField, ReadOnly] private float baseValue;
        [SerializeField, ReadOnly] private List<Modifier> modifiers;
        
        public MutableFloat( float baseValue )
        {
            this.baseValue = baseValue;
            totalValue = baseValue;
            modifiers = new List<Modifier>();
            OnTotalChanged = null;
        }

        public static implicit operator float( MutableFloat mutableFloat ) => mutableFloat!.totalValue;

        public event Action<float> OnTotalChanged;

        public void AddModifier( Modifier modifier )
        {
            modifiers.Add( modifier );
            CalculateTotalValue();
        }

        public bool TryRemoveModifier( Modifier modifier )
        {
            for( var i = modifiers.Count; i-- > 0; )
                if( modifiers[i].Equals( modifier ) )
                {
                    modifiers.RemoveAt( i );

                    CalculateTotalValue();
                    return true;
                }

            Debug.LogWarning( $"Modifier {modifier} not found" );
            return false;
        }

        public string ToString(string format) => totalValue.ToString( format );
        public string ToString(string format, IFormatProvider provider) => totalValue.ToString( format, provider );
        
        private void CalculateTotalValue()
        {
            ApplyModifiers( out var newTotal );

            //newTotal = Mathf.Clamp(newTotal, range.min, range.max);

            if( Mathf.Approximately( totalValue, newTotal ) )
                return;

            totalValue = newTotal;
            OnTotalChanged?.Invoke( totalValue );
        }

        private void ApplyModifiers( out float newTotal )
        {
            newTotal = baseValue;
            if( !modifiers.Any() )
                return;

            var overwriteMods = modifiers.Where( x => x.Type == ModifierType.Overwrite )
                .OrderByDescending( x => x );
            if( overwriteMods.Any() )
            {
                newTotal = overwriteMods.FirstOrDefault();
                return;
            }

            var flatAddModValue = modifiers.Where( x => x.Type == ModifierType.FlatAdd ).Sum( x => x );
            newTotal += flatAddModValue;

            var percentAddModValue = modifiers.Where( x => x.Type == ModifierType.PercentAdd ).Sum( x => x / 100f );
            newTotal *= 1 + percentAddModValue;

            var percentMultMods = modifiers.Where( x => x.Type == ModifierType.PercentMult );
            newTotal = percentMultMods.Aggregate( newTotal, ( current, mod ) => current * ( 1 + mod / 100f ) );
        }
    }

    internal interface IMutable<out T>
    {
        void AddModifier( Modifier modifier );
        bool TryRemoveModifier( Modifier modifier );
        event Action<T> OnTotalChanged;
    }
}