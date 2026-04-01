using System;
using System.Collections.Generic;
using Code.Data.Items.Shifter;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    [Serializable]
    public sealed class ShifterItem : TetrisItem, IShifterItem, IStatModifier
    {
        public WeaponUsageModifier usageMod { get; }
        public WeaponAttackModifier attackMod { get; }
        
        public IReadOnlyList<PawnStatModifier> Affixes => _affixes;
        private readonly List<PawnStatModifier> _affixes = new();

        public ShifterItem(ShifterConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            usageMod = new WeaponUsageModifier(
                config.usageStatMod.stat,
                new Modifier(config.usageStatMod.value, config.usageStatMod.type, Guid));
            
            attackMod = new WeaponAttackModifier(
                config.attackStatMod.stat,
                new Modifier(config.attackStatMod.value, config.attackStatMod.type, Guid));

            _affixes.Add(new PawnStatModifier(config.pawnStatMod.stat,
                    new Modifier(config.pawnStatMod.value, config.pawnStatMod.type, Guid)));
        }

        void IEquippable.OnEquipped(IPawnStats stats)
        {
            if (_affixes.Count == 0) return;
            foreach (var affix in _affixes)
                stats.ApplyMod(affix);
        }

        void IEquippable.OnUnequipped(IPawnStats stats)
        {
            if (_affixes.Count == 0) return;
            foreach (var affix in _affixes)
                stats.RemoveMod(affix);
        }
    }

    public interface IShifterItem : ITetrisItem
    {
        WeaponUsageModifier usageMod { get; }
        WeaponAttackModifier attackMod { get; }
    }
}