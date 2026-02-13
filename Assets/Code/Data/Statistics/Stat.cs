using System;
using NaughtyAttributes;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Data.Statistics
{
    [Serializable]
    public class Stat : ISerializationCallbackReceiver, IStat
    {
        [SerializeField, HideInInspector] protected string name;

        [field: SerializeField, ReadOnly] public StatType StatType { get; protected set; }

        [SerializeField, ReadOnly] protected MutableFloat TotalValue;

        public Stat( StatType stat, float baseValue )
        {
            StatType = stat;
            TotalValue = new MutableFloat( baseValue );
        }

        public static implicit operator float( Stat stat ) => stat.TotalValue;

        public void OnBeforeSerialize() => name = ToString();

        public void OnAfterDeserialize() {}

        public void AddModifier( Modifier modifier ) => TotalValue.AddModifier( modifier );
        public bool TryRemoveModifier( Modifier modifier ) => TotalValue.TryRemoveModifier( modifier );

        public virtual Stat GetDeepCopy()
        {
            var other = (Stat) MemberwiseClone();
            other.name = string.Copy( name );
            other.StatType = StatType;
            other.TotalValue = TotalValue;

            return other;
        }

        public sealed override string ToString()
        {
            var statName = StatType.ToDescription();

            if( statName.Contains( "Percent" ) )
                statName = statName.Replace( " Percent", "%" );

            return $"{statName}: {TotalValue:0.###}";
        }
    }

    internal interface IStat
    {
        StatType StatType { get; }
        void AddModifier( Modifier modifier );
        bool TryRemoveModifier( Modifier modifier );
        Stat GetDeepCopy();
    }
}