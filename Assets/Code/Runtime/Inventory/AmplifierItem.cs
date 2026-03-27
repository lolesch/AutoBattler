using System.Collections.Generic;
using Code.Data.Items.Amplifier;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public sealed class AmplifierItem : TetrisItem, IAmplifierItem, IStatModifier
    {
        public WeaponStatModifier              WeaponModifier { get; }
        public IReadOnlyList<PawnStatModifier> Affixes        => _affixes;

        private readonly List<PawnStatModifier> _affixes = new();

        public AmplifierItem(AmplifierConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            WeaponModifier = new WeaponStatModifier(
                config.WeaponStat,
                new Modifier(config.WeaponValue, config.WeaponModifierType, Guid));

            _affixes.Add(new PawnStatModifier(
                config.StatType,
                new Modifier(config.StatValue, config.ModifierType, Guid)));
        }

        public override void Use() { }

        void IEquippable.OnEquipped(IPawnStats stats)
        {
            foreach (var affix in _affixes)
                stats.ApplyMod(affix);
        }

        void IEquippable.OnUnequipped(IPawnStats stats)
        {
            foreach (var affix in _affixes)
                stats.RemoveMod(affix);
        }
    }

    public interface IAmplifierItem : ITetrisItem
    {
        WeaponStatModifier WeaponModifier { get; }
    }

    public interface IStatModifier : IEquippable
    {
        IReadOnlyList<PawnStatModifier> Affixes { get; }
    }
}