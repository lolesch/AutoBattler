using Code.Data.Items.Weapon;

namespace Code.Runtime.Container.Items
{
    public sealed class WeaponItem : TetrisItem, IWeaponItem
    {
        public float BaseDamage   { get; }
        public float AttackSpeed  { get; }
        public float ResourceCost { get; }

        public WeaponItem(WeaponConfig config, RotationType rotation) : base(config, rotation)
        {
            BaseDamage   = config.BaseDamage;
            AttackSpeed  = config.AttackSpeed;
            ResourceCost = config.ResourceCost;
        }

        public override void Use() { }
    }

    public interface IWeaponItem : ITetrisItem
    {
        float BaseDamage   { get; }
        float AttackSpeed  { get; }
        float ResourceCost { get; }
    }
}
