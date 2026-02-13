using System;
using Code.Data.Statistics;
using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Data.SO
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = Const.ConfigRoot + "Items")]
    public sealed class ItemConfig : ScriptableObject
    {
        [PreviewIcon] public Sprite icon;
        public StatType statType;
        public float value;
        public ModifierType modifierType;
        [SerializeField] private string debugStatModifierString;

        private void OnValidate()
        {
            debugStatModifierString = modifierType switch
            {
                ModifierType.Overwrite => $"= {value:0.###;-0.###}", //  = 123   | = -123   |  = 0
                ModifierType.FlatAdd => $"{value:+0.###;0.###;-0.###}", //   +123   |   -123   |    0
                ModifierType.PercentAdd => $"{value:+0.###;0.###;-0.###} %", //   +123 % |   -123 % |    0 %
                ModifierType.PercentMult => $"* {value:0.###;-0.###} %", //  * 123 % | * -123 % |  * 0 %

                var _ => $"?? {value:+ 0.###;- 0.###;0.###}",
            };
        }
    }
}