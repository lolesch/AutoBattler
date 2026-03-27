using System;
using Code.Data.Enums;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public static class WeaponUtils
    {
        public static MutableFloat GetFiringStat(IWeaponItem weapon, FiringStatType stat) => stat switch
        {
            FiringStatType.AttackSpeed  => weapon.AttackSpeed,
            FiringStatType.ResourceCost => weapon.ResourceCost,
            _                           => throw new ArgumentOutOfRangeException(nameof(stat), stat, null),
        };
        public static MutableFloat GetOutputStat(IWeaponItem weapon, AttackStatType stat) => stat switch
        {
            AttackStatType.Damage           => weapon.Damage,
            AttackStatType.ResourceGenOnHit => weapon.ResourceGenOnHit,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null),
        };
    }
}