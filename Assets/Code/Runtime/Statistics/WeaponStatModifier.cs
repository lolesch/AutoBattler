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
    public struct WeaponStatModifier : IWeaponStatModifier
    {
        [field: SerializeField] public AttackStatType AttackStat { get; private set; }
        [field: SerializeField] public Modifier       Modifier   { get; private set; }

        public WeaponStatModifier(AttackStatType attackStat, Modifier modifier)
        {
            AttackStat = attackStat;
            Modifier   = modifier;
        }
    }

    public interface IWeaponStatModifier
    {
        AttackStatType AttackStat { get; }
        Modifier       Modifier   { get; }
    }
}