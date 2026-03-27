using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Activator
{
    /// <summary>
    /// Unchained: applies a standalone pawn stat modifier (from StatItemConfig base).
    /// Chained: applies a conditional FiringStatType modifier to the attached weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "ActivatorConfig", menuName = "Configs/Items/Activator")]
    public sealed class ActivatorConfig : StatItemConfig
    {
        [field: Header("Chain — Firing Stat Modification")]
        [field: Tooltip("Only AttackSpeed and ResourceCost are valid targets for Activators.")]
        [field: SerializeField] public FiringStatType          WeaponStat           { get; private set; }
        [field: SerializeField] public float                   WeaponValue          { get; private set; }
        [field: SerializeField] public ModifierType            WeaponModifierType   { get; private set; }
        
        [field: Header("Chain — Output Stat Trade")]
        [field: Tooltip("The output stat traded against the firing stat modification.")]
        [field: SerializeField] public AttackStatType OutputStat          { get; private set; }
        [field: SerializeField] public float          OutputValue         { get; private set; }
        [field: SerializeField] public ModifierType   OutputModifierType  { get; private set; }
        
        [field: Header("Chain — Condition")]
        [field: SerializeField] public ActivatorConditionType  ConditionType        { get; private set; }
        [field: SerializeField] public float                   ConditionThreshold   { get; private set; }

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
            
            if (OutputValue == 0)
                Debug.LogWarning($"[{name}] Shifter OutputValue is 0 — no output trade configured. Every firing stat change must cost something on the other axis.", this);
        }
    }

    public enum ActivatorConditionType
    {
        Always,
        HpBelow,
        HpAbove,
        ResourceBelow,
        ResourceAbove,
        FirstXSeconds,    // stub — needs combat start timestamp
        EnemyCountBelow,  // stub — needs coordinator
        AllyCountBelow,   // stub — needs coordinator
        HasStatusEffect,  // stub — always false until status system exists
    }
}