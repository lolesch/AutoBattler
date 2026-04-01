using System.Collections.Generic;
using Code.Data.Enums;
using Code.Data.Items.Reactor;
using Code.Data.Items.Shifter;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public sealed class ReactorItem : TetrisItem, IReactorItem, IStatModifier
    {
        public ReactorType             ReactorType        { get; }
        public ConditionType  ConditionType      { get; }
        public float                   ConditionThreshold { get; }

        public IReadOnlyList<PawnStatModifier> Affixes => _affixes;
        private readonly List<PawnStatModifier> _affixes = new();

        public ReactorItem(ReactorConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            ReactorType        = config.ReactorType;
            ConditionType      = config.ConditionType;
            ConditionThreshold = config.ConditionThreshold;

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

    public interface IReactorItem : ITetrisItem
    {
        ReactorType            ReactorType        { get; }
        ConditionType ConditionType      { get; }
        float                  ConditionThreshold { get; }
    }
}