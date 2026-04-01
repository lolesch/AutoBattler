using System;
using System.Collections.Generic;
using Code.Data.Items;
using Code.Data.Items.Amplifier;
using Code.Data.Items.Reactor;
using Code.Data.Items.Shifter;
using Code.Data.Items.Weapon;
using Code.Runtime.Inventory;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Runtime.GameLoop
{
    /// <summary>
    /// Creates ITetrisItem runtime instances from ItemConfig data assets.
    /// Dispatches on config type — add a case here when new item types are introduced.
    /// RarityType is currently assigned randomly inside TetrisItem; weight it here later
    /// by filtering the pool before selection (e.g. by rarity tier, enemy level, etc.).
    /// </summary>
    public static class ItemFactory
    {
        public static ITetrisItem Create(IReadOnlyList<ItemConfig> pool)
        {
            if (pool == null || pool.Count == 0)
            {
                Debug.LogWarning("[ItemFactory] Pool is empty — no item created.");
                return null;
            }

            var config = pool[Random.Range(0, pool.Count)];
            return Create(config);
        }

        public static ITetrisItem Create(ItemConfig config) => config switch
        {
            WeaponConfig    c => new WeaponItem(c),
            AmplifierConfig c => new AmplifierItem(c),
            ShifterConfig c => new ShifterItem(c),
            ReactorConfig   c => new ReactorItem(c),
            _                 => throw new ArgumentOutOfRangeException(nameof(config), config.GetType().Name, "No factory mapping found.")
        };
    }
}