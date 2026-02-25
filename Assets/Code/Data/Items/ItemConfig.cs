using Code.Data.Enums;
using Submodules.Utility.Attributes;
using Submodules.Utility.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Data.Items
{
    [CreateAssetMenu(fileName = "ItemConfig", menuName = Const.ConfigRoot + "Items")]
    public sealed class ItemConfig : ScriptableObject, ITetrisItemData
    {
        [field: SerializeField, PreviewIcon] public Sprite Icon { get; private set; }
        public Vector2Int[] Shape { get; } = new[] { Vector2Int.zero };
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
    
    public interface IItemData
    {
        Sprite Icon { get; }
        // name
        // description
    }
    
    public interface ITetrisItemData : IItemData { Vector2Int[] Shape { get; } }
}