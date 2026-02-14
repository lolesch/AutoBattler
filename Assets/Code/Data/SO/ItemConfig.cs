using Code.Data.Enums;
using Submodules.Utility.Attributes;
using Submodules.Utility.Extensions;
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
            var mod = modifierType switch
            {
                ModifierType.Overwrite => $"= {value:0.###;-0.###}", //  = 123   | = -123   |  = 0
                ModifierType.FlatAdd => $"{value:+0.###;0.###;-0.###}", //   +123   |   -123   |    0
                ModifierType.PercentAdd => $"{value:+0.###;0.###;-0.###} %", //   +123 % |   -123 % |    0 %
                ModifierType.PercentMult => $"* {value:0.###;-0.###} %", //  * 123 % | * -123 % |  * 0 %

                var _ => $"?? {value:+ 0.###;- 0.###;0.###}",
            };
            debugStatModifierString = $"{statType.ToDescription()} {mod}";
        }
    }
}