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
        public Sprite Icon => config.icon;
        
        public Guid guid { get; } = Guid.NewGuid();
        [field: SerializeField] public RarityType RarityType { get; private set; }
        [field: SerializeField] public List<PawnStatModifier> Affixes { get; private set; } = new List<PawnStatModifier>();

        private RarityType GetRandomRarity() => (RarityType) Random.Range(0, Enum.GetValues( typeof(RarityType) ).Length);
        
        internal Item( ItemConfig config )
        {
            this.config = config;
            RarityType = GetRandomRarity();
            Affixes.Add( new PawnStatModifier( config.statType, new Modifier( config.value, config.modifierType, guid ) ) );
        }
    }
}