using UnityEngine;

namespace Code.Data.Items.Amplifier
{
    [CreateAssetMenu(fileName = "AmplifierConfig", menuName = "Configs/Items/Amplifier")]
    public sealed class AmplifierConfig : StatItemConfig
    {
        [Header("Amplifier Properties")]
        [Tooltip("Which weapon stat this amplifier scales. Uses Value and ModifierType from base config.")]
        [field: SerializeField] public WeaponStatType WeaponStat { get; private set; }
    }

    public enum WeaponStatType
    {
        Damage,
        AttackSpeed,
        ResourceCost,
    }
}