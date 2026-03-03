using System;
using Code.Data.Items.Amplifier;
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
        [field: SerializeField] public WeaponStatType WeaponStat { get; private set; }
        [field: SerializeField] public Modifier       Modifier   { get; private set; }

        public WeaponStatModifier(WeaponStatType weaponStat, Modifier modifier)
        {
            WeaponStat = weaponStat;
            Modifier   = modifier;
        }
    }

    public interface IWeaponStatModifier
    {
        WeaponStatType WeaponStat { get; }
        Modifier       Modifier   { get; }
    }
}