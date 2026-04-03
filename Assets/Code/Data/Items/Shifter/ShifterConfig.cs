using UnityEngine;

namespace Code.Data.Items.Shifter
{
    [CreateAssetMenu(fileName = "ShifterConfig", menuName = Const.ItemConfig + "Shifter")]
    public sealed class ShifterConfig : AttachmentItemConfig
    {
        [field: Header("Chained")]
        [field: SerializeField] public WeaponInputStatModConfig inputStatMod { get; private set; }
        [field: SerializeField] public WeaponOutputStatModConfig outputStatMod { get; private set; }
      
        public override int MaxConnectors => 2;
    }
}