using System;
using Code.Data.Enums;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public static class WeaponUtils
    {
        public static MutableFloat GetUsageStat(this IWeaponItem weapon, UsageStatType stat) => stat switch
        {
            UsageStatType.AttackSpeed  => weapon.AttackSpeed,
            UsageStatType.ManaCost => weapon.ResourceCost,
            _                           => throw new ArgumentOutOfRangeException(nameof(stat), stat, null),
        };
        public static MutableFloat GetAttackStat(this IWeaponItem weapon, AttackStatType result) => result switch
        {
            AttackStatType.Damage           => weapon.Damage,
            AttackStatType.ResourceGenOnHit => weapon.ResourceGenOnHit,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
    }
}