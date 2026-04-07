using System;
using Code.Data.Enums;
using Code.Runtime.Modules.Statistics;

namespace Code.Runtime.Modules.Inventory
{
    public static class WeaponUtils
    {
        public static MutableFloat GetInputStat(this IWeaponItem weapon, WeaponInputStat stat) => stat switch
        {
            WeaponInputStat.AttackSpeed  => weapon.AttackSpeed,
            WeaponInputStat.ManaCost => weapon.ResourceCost,
            _                           => throw new ArgumentOutOfRangeException(nameof(stat), stat, null),
        };
        public static MutableFloat GetOutputStat(this IWeaponItem weapon, WeaponOutputStat result) => result switch
        {
            WeaponOutputStat.Damage           => weapon.Damage,
            WeaponOutputStat.ResourceGenOnHit => weapon.ResourceGenOnHit,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null),
        };
    }
}