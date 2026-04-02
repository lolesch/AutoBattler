using Code.Data.Items.Amplifier;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public sealed class AmplifierItem : AttachmentItem, IAmplifierItem
    {
        public WeaponAttackModifier weaponAttackModifier { get; }

        public AmplifierItem(AmplifierConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            weaponAttackModifier = new WeaponAttackModifier(
                config.attackStatMod.stat,
                new Modifier(config.attackStatMod.value, config.attackStatMod.type, Guid));
        }
    }

    public interface IAmplifierItem : ITetrisItem
    {
        WeaponAttackModifier weaponAttackModifier { get; }
    }
}