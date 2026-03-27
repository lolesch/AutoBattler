using System.Collections.Generic;
using Code.Data.Enums;
using Code.Data.Items.Activator;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public sealed class ActivatorItem : TetrisItem, IActivatorItem, IStatModifier
    {
        public FiringStatType          FiringStat          { get; }
        public float                   FiringValue         { get; }
        public ModifierType            FiringModifierType  { get; }
        public ActivatorConditionType  ConditionType       { get; }
        public float                   ConditionThreshold  { get; }
        public AttackStatType          OutputStat          { get; }
        public float                   OutputValue         { get; }
        public ModifierType            OutputModifierType  { get; }

        public IReadOnlyList<PawnStatModifier> Affixes => _affixes;
        private readonly List<PawnStatModifier> _affixes = new();

        public ActivatorItem(ActivatorConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            FiringStat         = config.WeaponStat;
            FiringValue        = config.WeaponValue;
            FiringModifierType = config.WeaponModifierType;
            ConditionType      = config.ConditionType;
            ConditionThreshold = config.ConditionThreshold;
            OutputStat         = config.OutputStat;
            OutputValue        = config.OutputValue;
            OutputModifierType = config.OutputModifierType;

            if (config.StatValue != 0)
                _affixes.Add(new PawnStatModifier(config.StatType,
                    new Modifier(config.StatValue, config.ModifierType, Guid)));
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
        ActivatorConditionType ConditionType      { get; }
        float                  ConditionThreshold { get; }
        
        FiringStatType         FiringStat         { get; }
        float                  FiringValue        { get; }
        ModifierType           FiringModifierType { get; }
        
        AttackStatType         OutputStat         { get; }
        float                  OutputValue        { get; }
        ModifierType           OutputModifierType { get; }
    }
}