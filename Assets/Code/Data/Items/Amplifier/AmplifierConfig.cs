using UnityEngine;

namespace Code.Data.Items.Amplifier
{
    [CreateAssetMenu(fileName = "AmplifierConfig", menuName = Const.ItemConfig + "Amplifier")]
    public sealed class AmplifierConfig : AttachmentItemConfig
    {
        [field: Header("Chained")]
        [field: SerializeField] public WeaponOutputStatModConfig outputStatMod { get; private set; }

        public override int MaxConnectors => 2;
    }
}