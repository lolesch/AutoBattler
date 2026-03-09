using System.Collections.Generic;
using Code.Data.Items;
using Code.Data.Items.Amplifier;
using Code.Data.Items.Weapon;
using Code.Runtime.Inventory;
using UnityEngine;

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

        public static ITetrisItem Create(ItemConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("[ItemFactory] Null config passed — no item created.");
                return null;
            }

            // Rotation is None by default; randomize here when visual variety is desired.
            const RotationType rotation = RotationType.None;

            return config switch
            {
                WeaponConfig    weapon => new WeaponItem(weapon, rotation),
                AmplifierConfig amp    => new AmplifierItem(amp, rotation),
                _ => LogUnknown(config),
            };
        }

        private static ITetrisItem LogUnknown(ItemConfig config)
        {
            Debug.LogWarning($"[ItemFactory] Unknown ItemConfig type: {config.GetType().Name}. Add a case to ItemFactory.Create().");
            return null;
        }
    }
}