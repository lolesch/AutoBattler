using System;
using Code.Data.Enums;
using Code.Data.Pawns;
using Code.Runtime.Statistics;
using NaughtyAttributes;
using UnityEngine;

namespace Code.Runtime
{
    [Serializable]
    public sealed class PawnStats
    {
        [field: SerializeField, ReadOnly, AllowNesting] public Resource health { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Resource Mana { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Stat damage { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Stat attackSpeed { get; private set; }

        // UNIT STATS:
        /* Unit stats fall under an offensive or defensive category
         * on the most abstract level each unit can attack and be attacked
         * Units
         */
        
        // ITEMS:
        /* Items can act as flat stat increases -> +x health while equipped
         * or trigger mods on a timer/condition
         * 
         */
        
        public PawnStats( PawnConfig config )
        {
            health = new Resource( StatType.MaxLife, config.baseHealth );
            Mana = new Resource( StatType.MaxMana, config.baseMana );
            damage = new Stat( StatType.Damage, config.baseDamage );
            attackSpeed = new Stat( StatType.AttackSpeed, config.baseAttackSpeed );
        }

        private Stat GetStat( StatType type ) => type switch
        {
            StatType.Damage => damage,
            StatType.MaxLife => health,
            StatType.AttackSpeed => attackSpeed,
            StatType.MaxMana => Mana,
            
            _ => throw new ArgumentOutOfRangeException( nameof( type ), type, null )
        };

        public void ApplyMod( PawnStatModifier mod ) => GetStat( mod.stat )?.AddModifier( mod.modifier );
    }
}