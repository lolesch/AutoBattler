using System;
using NaughtyAttributes;
using UnityEngine;
using Code.Data.Enums;
using Submodules.Utility.Extensions;
using UnityEngine.Serialization;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public class Stat : IStat
    {
        [SerializeField, HideInInspector] protected string name;

        [field: FormerlySerializedAs("<StatType>k__BackingField")] [field: SerializeField, ReadOnly] public PawnStatType pawnStatType { get; protected set; }

        [SerializeField, ReadOnly] protected MutableFloat MaxValue;

        public Stat( PawnStatType pawnStat, float baseValue )
        {
            pawnStatType = pawnStat;
            MaxValue = new MutableFloat( baseValue );
        }

        public static implicit operator float( Stat stat ) => stat.MaxValue;

        public void AddModifier( Modifier modifier ) => MaxValue.AddModifier( modifier );
        public bool TryRemoveModifier( Modifier modifier ) => MaxValue.TryRemoveModifier( modifier );
        
        //public bool TryRemoveAllModifiersBySource( IModifierSource source ) => MaxValue.TryRemoveAllModifiersBySource( source.guid );

        public virtual Stat GetDeepCopy()
        {
            var other = (Stat) MemberwiseClone();
            other.name = string.Copy( name );
            other.pawnStatType = pawnStatType;
            other.MaxValue = MaxValue;

            return other;
        }

        public sealed override string ToString()
        {
            var statName = pawnStatType.ToDescription();

            if( statName.Contains( "Percent" ) )
                statName = statName.Replace( " Percent", "%" );

            return $"{statName}: {MaxValue:0.###}";
        }
    }

    internal interface IStat
    {
        PawnStatType pawnStatType { get; }
        void AddModifier( Modifier modifier );
        bool TryRemoveModifier( Modifier modifier );
        Stat GetDeepCopy();
    }
    
    public interface IModifierSource {
        Guid guid { get; }
    }
}