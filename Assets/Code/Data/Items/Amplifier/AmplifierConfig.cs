using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Amplifier
{
    [CreateAssetMenu(fileName = "AmplifierConfig", menuName = Const.ConfigRoot + "Items/Amplifier")]
    public sealed class AmplifierConfig : StatItemConfig, IAmplifierConfig
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