using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Runtime.Inventory
{
    public sealed class ItemChain : IItemChain
    {
        public static readonly IItemChain Empty = new ItemChain(null, new List<ITetrisItem>());

        public ITetrisItem                Root      { get; }
        public IReadOnlyList<ITetrisItem> Modifiers { get; }
        public bool                       IsValid   => Root != null;
        public IWeaponItem                Weapon    => Root as IWeaponItem
                                                    ?? Modifiers.OfType<IWeaponItem>().FirstOrDefault();

        private bool _modifiersApplied;

        public ItemChain(ITetrisItem root, List<ITetrisItem> modifiers)
        {
            Root      = root;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Applies amplifier modifiers to the weapon's attack stats.
        /// Idempotent — safe to call multiple times; applies once until RemoveChainModifiers is called.
        /// </summary>
        public void ApplyChainModifiers()
        {
            if (_modifiersApplied) return;

            var weapon = Weapon;
            if (weapon == null)
            {
                Debug.LogWarning("ItemChain.ApplyChainModifiers: no weapon in chain.");
                return;
            }

            foreach (var item in Modifiers)
            {
                if (item is not IAmplifierItem amp) continue;
                var mod = amp.WeaponModifier;
                
                WeaponUtils.GetOutputStat(weapon, mod.AttackStat)
                    .AddModifier(mod.Modifier);
            }

            _modifiersApplied = true;
        }

        /// <summary>Removes all amplifier modifiers from the weapon's attack stats.</summary>
        public void RemoveChainModifiers()
        {
            if (!_modifiersApplied) return;

            var weapon = Weapon;
            if (weapon == null) return;

            foreach (var item in Modifiers)
            {
                if (item is not IAmplifierItem amp) continue;
            
                var mod = amp.WeaponModifier;
                WeaponUtils.GetOutputStat(weapon, mod.AttackStat)
                    .TryRemoveModifier(mod.Modifier);
            }

            _modifiersApplied = false;
        }
    }

    public interface IItemChain
    {
        ITetrisItem               Root      { get; }
        IReadOnlyList<ITetrisItem> Modifiers { get; }
        bool                      IsValid   { get; }
        IWeaponItem               Weapon    { get; }
        void ApplyChainModifiers();
        void RemoveChainModifiers();
    }
}