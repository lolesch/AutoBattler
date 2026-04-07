using System;
using System.Collections.Generic;
using Code.Data.Items;
using Code.Data.Items.Amplifier;
using Code.Data.Items.Converter;
using Code.Data.Items.Reactor;
using Code.Data.Items.Shifter;
using Code.Data.Items.Weapon;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Runtime.Inventory
{
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
            ShifterConfig   c => new ShifterItem(c),
            ReactorConfig   c => new ReactorItem(c),
            ConverterConfig c => new ConverterItem(c),
            _                 => throw new ArgumentOutOfRangeException(nameof(config), config.GetType().Name, "No factory mapping found.")
        };
    }
}