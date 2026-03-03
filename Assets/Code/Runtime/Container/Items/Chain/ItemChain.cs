using System.Collections.Generic;
using Code.Data.Items.Amplifier;
using Code.Runtime.Statistics;
using UnityEngine;

namespace Code.Runtime.Container.Items.Chain
{
    public sealed class ItemChain : IItemChain
    {
        public static readonly IItemChain Empty = new ItemChain(null, new List<ITetrisItem>());

        public IWeaponItem               Weapon    { get; }
        public IReadOnlyList<ITetrisItem> Modifiers { get; }
        public bool                      IsValid   => Weapon != null;

        public ItemChain(IWeaponItem weapon, List<ITetrisItem> modifiers)
        {
            Weapon    = weapon;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Resolves the chain by feeding each amplifier's WeaponStatModifier into
        /// a MutableFloat per stat. MutableFloat.ApplyModifiers handles all modifier
        /// math — no duplication of that logic here.
        /// </summary>
        public IResolvedWeaponStats Resolve()
        {
            if (!IsValid)
            {
                Debug.LogWarning("ItemChain.Resolve() called on an empty chain.");
                return new ResolvedWeaponStats(0f, 0f, 0f);
            }

            var damage       = new MutableFloat(Weapon.BaseDamage);
            var attackSpeed  = new MutableFloat(Weapon.AttackSpeed);
            var resourceCost = new MutableFloat(Weapon.ResourceCost);

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
        IWeaponItem               Weapon    { get; }
        IReadOnlyList<ITetrisItem> Modifiers { get; }
        bool                      IsValid   { get; }
        IResolvedWeaponStats      Resolve();
    }
}