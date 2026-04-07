using System;
using Code.Data.Items.Shifter;
using Code.Runtime.Modules.Statistics;

namespace Code.Runtime.Modules.Inventory
{
    [Serializable]
    public sealed class ShifterItem : AttachmentItem, IShifterItem
    {
        public WeaponInputModifier inputMod { get; }
        public WeaponOutputModifier outputMod { get; }
        
        public ShifterItem(ShifterConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            inputMod = new WeaponInputModifier(
                config.inputStatMod.stat,
                new Modifier(config.inputStatMod.value, config.inputStatMod.type, Guid));
            
            outputMod = new WeaponOutputModifier(
                config.outputStatMod.stat,
                new Modifier(config.outputStatMod.value, config.outputStatMod.type, Guid));
        }
    }

    public interface IShifterItem : ITetrisItem
    {
        WeaponInputModifier inputMod { get; }
        WeaponOutputModifier outputMod { get; }
    }
}