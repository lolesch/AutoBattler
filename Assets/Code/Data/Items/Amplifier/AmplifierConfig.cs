using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Amplifier
{
    [CreateAssetMenu(fileName = "AmplifierConfig", menuName = Const.ItemConfig + "Amplifier")]
    public sealed class AmplifierConfig : AttachmentItemConfig, IAmplifierConfig
    {
        [field: Header("Chained")]
        [field: SerializeField] public WeaponAttackStatModConfig attackStatMod { get; private set; }

        public override int MaxConnectors => 2;
    }

    public interface IAmplifierConfig
    {
        WeaponAttackStatModConfig attackStatMod { get; }
    }
}