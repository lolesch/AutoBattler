using System;
using Code.Data.Items.Shifter;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    [Serializable]
    public sealed class ShifterItem : AttachmentItem, IShifterItem
    {
        public WeaponUsageModifier usageMod { get; }
        public WeaponAttackModifier attackMod { get; }
        
        public ShifterItem(ShifterConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            usageMod = new WeaponUsageModifier(
                config.usageStatMod.stat,
                new Modifier(config.usageStatMod.value, config.usageStatMod.type, Guid));
            
            attackMod = new WeaponAttackModifier(
                config.attackStatMod.stat,
                new Modifier(config.attackStatMod.value, config.attackStatMod.type, Guid));
        }
    }

    public interface IShifterItem : ITetrisItem
    {
        WeaponUsageModifier usageMod { get; }
        WeaponAttackModifier attackMod { get; }
    }
}