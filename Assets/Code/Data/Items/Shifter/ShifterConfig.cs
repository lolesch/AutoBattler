using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Shifter
{
    [CreateAssetMenu(fileName = "ShifterConfig", menuName = Const.ConfigRoot + "Items/Shifter")]
    public sealed class ShifterConfig : StatItemConfig, IShifterConfig
    {
        [field: Header("Chained")]
        [field: SerializeField] public WeaponUsageStatModConfig usageStatMod { get; private set; }
        [field: SerializeField] public WeaponAttackStatModConfig attackStatMod { get; private set; }
      
        public override int MaxConnectors => 2;
    }

    public interface IShifterConfig
    {
        WeaponUsageStatModConfig usageStatMod { get; }
        WeaponAttackStatModConfig attackStatMod { get; }
    }
}