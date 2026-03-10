using System.Collections.Generic;
using System.Linq;
using Code.Data.Items.Amplifier;
using Code.Runtime.Statistics;
using UnityEngine;

namespace Code.Runtime.Inventory
{
    /// <summary>
    /// Marker interface for items that can root a chain — Weapon, Activator, Reactor.
    /// A root item starts one traversal per connected connector.
    /// Will gain firing behaviour when Activators and Reactors are implemented.
    /// </summary>
    public interface IChainRoot : ITetrisItem { }

    public sealed class ItemChain : IItemChain
    {
        public static readonly IItemChain Empty = new ItemChain(null, new List<ITetrisItem>());

        public IChainRoot                Root      { get; }
        public IReadOnlyList<ITetrisItem> Modifiers { get; }
        public bool                      IsValid   => Root != null;

        public ItemChain(IChainRoot root, List<ITetrisItem> modifiers)
        {
            Root      = root;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Finds the first weapon in the chain (root or modifiers) and resolves
        /// stats by feeding amplifiers into MutableFloat per stat.
        /// Root may be an Activator/Reactor — the weapon is then in Modifiers.
        /// </summary>
        public IResolvedWeaponStats Resolve()
        {
            if (!IsValid)
            {
                Debug.LogWarning("ItemChain.Resolve() called on an empty chain.");
                return new ResolvedWeaponStats(0f, 0f, 0f);
            }

            var weapon = Root as IWeaponItem
                      ?? Modifiers.OfType<IWeaponItem>().FirstOrDefault();

            if (weapon == null)
            {
                Debug.LogWarning("ItemChain.Resolve() found no weapon in chain.");
                return new ResolvedWeaponStats(0f, 0f, 0f);
            }

            var damage       = new MutableFloat(weapon.BaseDamage);
            var attackSpeed  = new MutableFloat(weapon.AttackSpeed);
            var resourceCost = new MutableFloat(weapon.ResourceCost);

            foreach (var item in Modifiers)
            {
                if (item is not IAmplifierItem amp)
                    continue;

                var mod = amp.WeaponModifier;
                switch (mod.WeaponStat)
                {
                    case WeaponStatType.Damage:       damage.AddModifier(mod.Modifier);       break;
                    case WeaponStatType.AttackSpeed:  attackSpeed.AddModifier(mod.Modifier);  break;
                    case WeaponStatType.ResourceCost: resourceCost.AddModifier(mod.Modifier); break;
                }
            }

            return new ResolvedWeaponStats(damage, attackSpeed, resourceCost);
        }
    }

    public interface IItemChain
    {
        IChainRoot                Root      { get; }
        IReadOnlyList<ITetrisItem> Modifiers { get; }
        bool                      IsValid   { get; }
        IResolvedWeaponStats      Resolve();
    }
}