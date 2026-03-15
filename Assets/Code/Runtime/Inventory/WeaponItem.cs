using Code.Data.Enums;
using Code.Data.Items.Weapon;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    public sealed class WeaponItem : TetrisItem, IWeaponItem
    {
        public MutableFloat          Damage                    { get; }
        public MutableFloat          AttackSpeed               { get; }
        public MutableFloat          ResourceCost              { get; }
        public MutableFloat          ResourceGenOnHit          { get; }
        public PayloadConditionType  PayloadCondition          { get; }
        public float                 PayloadDamageMultiplier   { get; }
        public float                 PayloadConditionThreshold { get; }

        public WeaponItem(WeaponConfig config, RotationType rotation = RotationType.None) : base(config, rotation)
        {
            Damage                    = new MutableFloat(config.BaseDamage);
            AttackSpeed               = new MutableFloat(config.AttackSpeed);
            ResourceCost              = new MutableFloat(config.ResourceCost);
            ResourceGenOnHit          = new MutableFloat(config.ResourceGenOnHit);
            PayloadCondition          = config.PayloadCondition;
            PayloadDamageMultiplier   = config.PayloadDamageMultiplier;
            PayloadConditionThreshold = config.PayloadConditionThreshold;
        }

        public override void Use() { }
    }

    public interface IWeaponItem : ITetrisItem
    {
        MutableFloat         Damage                    { get; }
        MutableFloat         AttackSpeed               { get; }
        MutableFloat         ResourceCost              { get; }
        MutableFloat         ResourceGenOnHit          { get; }
        PayloadConditionType PayloadCondition          { get; }
        float                PayloadDamageMultiplier   { get; }
        float                PayloadConditionThreshold { get; }
    }
}