using System;
using Submodules.Utility.SerializeInterface;
using UnityEngine;

namespace Code.Data.Statistics
{
    [Serializable]
    public struct Modifier : IComparable<Modifier>, IEquatable<Modifier>
    {
        // TODO: add source to IModifier -> support removal of all modifiers from source

        [SerializeField] private float value;
        [SerializeField] private ModifierType type;
        [field: SerializeField] private InterfaceReference<IModifierSource> source { get; }

        public Modifier( float value, ModifierType type, IModifierSource source )
        {
            this.value = value;
            this.type = type;
            this.source = new InterfaceReference<IModifierSource>();
            this.source.Value = source;
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
            source.Value == other.source.Value && Type == other.Type && Mathf.Approximately( value, other.value );
    }
    
    public interface IModifierSource {}
}