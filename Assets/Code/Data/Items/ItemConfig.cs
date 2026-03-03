using System.Collections.Generic;
using Code.Data.Enums;
using Code.Runtime.Grids.RectGridInspector;
using Submodules.Utility.Attributes;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Data.Items
{
    /// <summary>
    /// Base config for all grid-placed items.
    /// Holds shape and chain connectors only — no stat modifier data.
    /// Subclass StatItemConfig for items that also apply standalone stat effects.
    /// </summary>
    public abstract class ItemConfig : ScriptableObject, IItemData
    {
        [field: SerializeField, PreviewIcon] public Sprite       Icon  { get; private set; }
        [field: SerializeField]              public RectGridBool Shape { get; private set; }

        [Header("Chain Connectors")]
        [Tooltip("Connection points on this item. Each defines a local cell and the direction it reaches toward.")]
        [SerializeField] private List<ChainConnector> connectors = new();

        public IReadOnlyList<ChainConnector> Connectors => connectors;
    }

    /// <summary>
    /// Config for items that apply a standalone stat effect when placed in the inventory.
    /// Amplifiers and plain stat items extend this. Weapons do not.
    /// </summary>
    public abstract class StatItemConfig : ItemConfig, IStatItemData
    {
        [Header("Standalone Effect")]
        [field: SerializeField] public StatType     StatType     { get; private set; }
        [field: SerializeField] public float        Value        { get; private set; }
        [field: SerializeField] public ModifierType ModifierType { get; private set; }

        [SerializeField, HideInInspector] private string debugStatModifierString;

        private void OnValidate()
        {
            var mod = ModifierType switch
            {
                ModifierType.Overwrite   => $"= {Value:0.###;-0.###}",
                ModifierType.FlatAdd     => $"{Value:+0.###;0.###;-0.###}",
                ModifierType.PercentAdd  => $"{Value:+0.###;0.###;-0.###} %",
                ModifierType.PercentMult => $"* {Value:0.###;-0.###} %",
                var _                    => $"?? {Value:+ 0.###;- 0.###;0.###}",
            };
            debugStatModifierString = $"{StatType.ToDescription()} {mod}";
        }
    }

    public interface IItemData
    {
        Sprite                        Icon       { get; }
        RectGridBool                  Shape      { get; }
        IReadOnlyList<ChainConnector> Connectors { get; }
    }

    public interface IStatItemData : IItemData
    {
        StatType     StatType     { get; }
        float        Value        { get; }
        ModifierType ModifierType { get; }
    }
}