using System;
using Code.Data.Enums;
using Code.Data.SO;
using Code.Runtime.Statistics;
using NaughtyAttributes;
using UnityEngine;

namespace Code.Runtime
{
    [Serializable]
    public sealed class PawnStats
    {
        [field: SerializeField, ReadOnly, AllowNesting] public Resource health { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Stat damage { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Stat attackSpeed { get; private set; }

        public PawnStats( PawnConfig config )
        {
            health = new Resource( StatType.MaxLife, config.baseHealth );
            damage = new Stat( StatType.Damage, config.baseDamage );
            attackSpeed = new Stat( StatType.AttackSpeed, config.baseAttackSpeed );
        }

        private Stat GetStat( StatType type ) => type switch
        {
            StatType.Damage => damage,
            StatType.MaxLife => health,
            StatType.AttackSpeed => attackSpeed,
            
            _ => throw new ArgumentOutOfRangeException( nameof( type ), type, null )
        };

        public void ApplyMod( PawnStatModifier mod ) => GetStat( mod.stat )?.AddModifier( mod.modifier );
    }
}