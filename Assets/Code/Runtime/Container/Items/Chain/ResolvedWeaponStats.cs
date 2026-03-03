namespace Code.Runtime.Container.Items.Chain
{
    /// <summary>
    /// The fully resolved stats of a weapon after all chain modifications are applied.
    /// This is what the combat system reads — it never touches items directly.
    /// </summary>
    public sealed class ResolvedWeaponStats : IResolvedWeaponStats
    {
        public float Damage       { get; set; }
        public float AttackSpeed  { get; set; }
        public float ResourceCost { get; set; }

        public ResolvedWeaponStats(float damage, float attackSpeed, float resourceCost)
        {
            Damage       = damage;
            AttackSpeed  = attackSpeed;
            ResourceCost = resourceCost;
        }
    }

    public interface IResolvedWeaponStats
    {
        float Damage       { get; }
        float AttackSpeed  { get; }
        float ResourceCost { get; }
    }
}
