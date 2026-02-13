using System;
using Code.Data.SO;
using Code.Data.Statistics;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Runtime
{
    [Serializable]
    public sealed class Item : IModifierSource
    {
        [SerializeField] private ItemConfig config;
        
        [SerializeField, ReadOnly, PreviewIcon] private Sprite icon;
        [SerializeField] private StatModifier modifier;

        internal Item( ItemConfig config )
        {
            this.config = config;
            icon = config.icon;
        }
        
        public void Initialize() => modifier = new StatModifier( config.statType, new Modifier( config.value, config.modifierType, this ) );
    }
}