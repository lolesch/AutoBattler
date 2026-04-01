using System.Collections.Generic;
using Code.Data.Items.Amplifier;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public sealed class AmplifierItem : TetrisItem, IAmplifierItem, IStatModifier
    {
        public WeaponAttackModifier              weaponAttackModifier { get; }
        public IReadOnlyList<PawnStatModifier> Affixes        => _affixes;

        private readonly List<PawnStatModifier> _affixes = new();

        public AmplifierItem(AmplifierConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            weaponAttackModifier = new WeaponAttackModifier(
                config.attackStatMod.stat,
                new Modifier(config.attackStatMod.value, config.attackStatMod.type, Guid));

            _affixes.Add(new PawnStatModifier(
                config.pawnStatMod.stat,
                new Modifier(config.pawnStatMod.value, config.pawnStatMod.type, Guid)));
        }

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
        WeaponAttackModifier weaponAttackModifier { get; }
    }
}