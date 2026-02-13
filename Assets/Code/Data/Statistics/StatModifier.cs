using System;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Data.Statistics
{
    [Serializable]
    public sealed class StatModifier
    {
        [SerializeField] private string name;
        public readonly StatType statType;
        public readonly Modifier modifier;

        public StatModifier( StatType statType, Modifier modifier )
        {
            this.statType = statType;
            this.modifier = modifier;

            name = ToString();
        }

        public override string ToString() => $"{statType.ToDescription()} {modifier}";
    }
}