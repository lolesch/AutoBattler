using Code.Data.Enums;
using Code.Data.Items.Weapon;

namespace Code.Runtime.Inventory
{
    public sealed class WeaponItem : TetrisItem, IWeaponItem
    {
        public float                BaseDamage                { get; }
        public float                AttackSpeed               { get; }
        public float                ResourceCost              { get; }
        public PayloadConditionType PayloadCondition          { get; }
        public float                PayloadDamageMultiplier   { get; }
        public float                PayloadConditionThreshold { get; }

        public WeaponItem(WeaponConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            BaseDamage                = config.BaseDamage;
            AttackSpeed               = config.AttackSpeed;
            ResourceCost              = config.ResourceCost;
            PayloadCondition          = config.PayloadCondition;
            PayloadDamageMultiplier   = config.PayloadDamageMultiplier;
            PayloadConditionThreshold = config.PayloadConditionThreshold;
        }

        public override void Use() { }
    }

    public interface IWeaponItem : ITetrisItem, IChainRoot
    {
        float                BaseDamage                { get; }
        float                AttackSpeed               { get; }
        float                ResourceCost              { get; }
        PayloadConditionType PayloadCondition          { get; }
        float                PayloadDamageMultiplier   { get; }
        float                PayloadConditionThreshold { get; }
    }
}
