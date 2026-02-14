using System;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public struct Modifier : IComparable<Modifier>, IEquatable<Modifier>
    {
        [SerializeField] private float value;
        [SerializeField] private ModifierType type;
        [field:SerializeField] public Guid source { get; }

        public Modifier( float value, ModifierType type, Guid source )
        {
            this.value = value;
            this.type = type;
            this.source = source;
        }
        
        public readonly ModifierType Type => type;

        public static implicit operator float( Modifier mod ) => mod.value;

        public readonly int CompareTo( Modifier other )
        {
            var typeComparison = Type.CompareTo( other.Type );

            return typeComparison != 0 ? typeComparison : value.CompareTo( other.value );
        }

        public readonly override string ToString() => Type switch
        {
            ModifierType.Overwrite => $"= {value:0.###;-0.###}",           //  = 123   | = -123   |  = 0
            ModifierType.FlatAdd => $"{value:+0.###;0.###;-0.###}",        //   +123   |   -123   |    0
            ModifierType.PercentAdd => $"{value:+0.###;0.###;-0.###} %",   //   +123 % |   -123 % |    0 %
            ModifierType.PercentMult => $"* {value:0.###;-0.###} %",       //  * 123 % | * -123 % |  * 0 %

            var _ => $"?? {value:+ 0.###;- 0.###;0.###}",
        };

        public readonly bool Equals( Modifier other ) =>
            source == other.source && Type == other.Type && Mathf.Approximately( value, other.value );
    }
    
    public interface IModifierSource {
        Guid guid { get; }
    }
}