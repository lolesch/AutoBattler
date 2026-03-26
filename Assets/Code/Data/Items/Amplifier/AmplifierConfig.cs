using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Amplifier
{
    [CreateAssetMenu(fileName = "AmplifierConfig", menuName = "Configs/Items/Amplifier")]
    public sealed class AmplifierConfig : StatItemConfig
    {
        [field: Header("Chain Effect")]
        [field: Tooltip("Which weapon stat this amplifier modifies when connected in a chain.")]
        [field: SerializeField] public AttackStatType WeaponStat { get; private set; }
        [field: SerializeField] public float          WeaponValue         { get; private set; }
        [field: SerializeField] public ModifierType   WeaponModifierType  { get; private set; }

        [SerializeField, HideInInspector] private string debugWeaponModifierString;

        protected override int MaxConnectors => 2;

        protected override void OnValidate()
        {
            base.OnValidate();

            var mod = WeaponModifierType switch
            {
                ModifierType.Overwrite   => $"= {WeaponValue:0.###;-0.###}",
                ModifierType.FlatAdd     => $"{WeaponValue:+0.###;0.###;-0.###}",
                ModifierType.PercentAdd  => $"{WeaponValue:+0.###;0.###;-0.###} %",
                ModifierType.PercentMult => $"* {WeaponValue:0.###;-0.###} %",
                var _                    => $"?? {WeaponValue:+0.###;-0.###;0.###}",
            };
            debugWeaponModifierString = $"{WeaponStat} {mod}";
        }
    }
}