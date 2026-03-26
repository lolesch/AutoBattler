using System.Collections.Generic;
using Code.Data.Enums;
using Code.Data.Items.Activator;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public sealed class ActivatorItem : TetrisItem, IActivatorItem, IStatModifier
    {
        public FiringStatType          WeaponStat          { get; }
        public float                   WeaponValue         { get; }
        public ModifierType            WeaponModifierType  { get; }
        public ActivatorConditionType  ConditionType       { get; }
        public float                   ConditionThreshold  { get; }

        public IReadOnlyList<PawnStatModifier> Affixes => _affixes;
        private readonly List<PawnStatModifier> _affixes = new();

        public ActivatorItem(ActivatorConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            WeaponStat         = config.WeaponStat;
            WeaponValue        = config.WeaponValue;
            WeaponModifierType = config.WeaponModifierType;
            ConditionType      = config.ConditionType;
            ConditionThreshold = config.ConditionThreshold;

            if (config.Value != 0)
                _affixes.Add(new PawnStatModifier(config.StatType,
                    new Modifier(config.Value, config.ModifierType, Guid)));
        }

        public override void Use() { }

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

    public interface IActivatorItem : ITetrisItem
    {
        FiringStatType         WeaponStat         { get; }
        float                  WeaponValue        { get; }
        ModifierType           WeaponModifierType { get; }
        ActivatorConditionType ConditionType      { get; }
        float                  ConditionThreshold { get; }
    }
}