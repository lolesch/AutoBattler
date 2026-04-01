using System;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Runtime.Statistics
{
    /// <summary>
    /// Mirrors PawnStatModifier but targets weapon chain stats instead of pawn stats.
    /// Used by AmplifierItem to modify weapon properties during chain resolution.
    /// </summary>
    [Serializable]
    public class WeaponModifier<T> where T : Enum
    {
        [field: SerializeField] public T stat { get; private set; }
        [field: SerializeField] public Modifier modifier { get; private set; }

        public WeaponModifier(T stat, Modifier modifier)
        {
            this.stat = stat;
            this.modifier   = modifier;
        }
    }
    
    [Serializable]
    public class WeaponUsageModifier : WeaponModifier<UsageStatType>
    {
        public WeaponUsageModifier(UsageStatType stat, Modifier modifier) : base(stat, modifier) { }
    }
    
    [Serializable]
    public class WeaponAttackModifier : WeaponModifier<AttackStatType>
    {
        public WeaponAttackModifier(AttackStatType stat, Modifier modifier) : base(stat, modifier) { }
    }
}