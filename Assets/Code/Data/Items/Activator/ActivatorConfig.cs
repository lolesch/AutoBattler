using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Activator
{
    /// <summary>
    /// Activator applies a conditional stat modifier to the attached weapon's firing stats.
    /// The weapon always fires on its default timer — the Activator modifies AttackSpeed or
    /// ResourceCost while its condition is continuously true.
    /// Does not extend StatItemConfig — no standalone pawn stat effect.
    /// </summary>
    [CreateAssetMenu(fileName = "ActivatorConfig", menuName = "Configs/Items/Activator")]
    public sealed class ActivatorConfig : ItemConfig
    {
        [Header("Activator — Stat Modification")]
        [Tooltip("Only AttackSpeed and ResourceCost are valid targets for Activators.")]
        [field: SerializeField] public FiringStatType          WeaponStat           { get; private set; }
        [field: SerializeField] public float                   Value                { get; private set; }
        [field: SerializeField] public ModifierType            ModifierType         { get; private set; }

        [Header("Activator — Condition")]
        [field: SerializeField] public ActivatorConditionType  ConditionType        { get; private set; }
        [field: SerializeField] public float                   ConditionThreshold   { get; private set; }

        protected override int MaxConnectors => 2;
    }

    public enum ActivatorConditionType
    {
        Always,           // unconditional — modifier always applies
        HpBelow,          // owning unit HP% < threshold
        HpAbove,          // owning unit HP% > threshold
        ResourceBelow,    // owning unit resource% < threshold
        ResourceAbove,    // owning unit resource% > threshold
        FirstXSeconds,    // active during first X seconds of combat (threshold = seconds)
        EnemyCountBelow,  // fewer than X enemies alive (threshold = count)
        AllyCountBelow,   // fewer than X allies alive (threshold = count)
        HasStatusEffect,  // stub — always false until status system exists
    }
}