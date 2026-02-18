using System;
using System.Collections.Generic;
using Code.Data.Enums;
using Code.Data.Items;
using Code.Runtime.Statistics;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Runtime
{
    [Serializable]
    public sealed class Item : IModifierSource
    {
        [SerializeField] private ItemConfig config;
        [SerializeField, ReadOnly, PreviewIcon] private Sprite icon;
        
        [field: SerializeField] public Guid guid { get; } = Guid.NewGuid();
        [field: SerializeField] public RarityType RarityType { get; }
        [field: SerializeField] public List<PawnStatModifier> Affixes { get; } = new List<PawnStatModifier>();

        private RarityType GetRandomRarity() => (RarityType) Random.Range(0, Enum.GetValues( typeof(RarityType) ).Length);
        
        internal Item( ItemConfig config )
        {
            this.config = config;
            icon = config.icon;
            RarityType = GetRandomRarity();
            Affixes.Add( new PawnStatModifier( config.statType, new Modifier( config.value, config.modifierType, guid ) ) );
        }
    }
}