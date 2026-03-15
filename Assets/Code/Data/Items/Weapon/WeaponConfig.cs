using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Weapon
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Configs/Items/Weapon")]
    public sealed class WeaponConfig : ItemConfig
    {
        [Header("Weapon Properties")]
        [field: SerializeField] public float BaseDamage   { get; private set; }
        [field: SerializeField] public float AttackSpeed  { get; private set; }
        [field: SerializeField] public float ResourceCost { get; private set; }
        [field: SerializeField] public float ResourceGenOnHit { get; private set; }

        [Header("Payload")]
        [Tooltip("Condition that must be met for this weapon to fire as a mid-chain payload.")]
        [field: SerializeField] public PayloadConditionType PayloadCondition          { get; private set; }
        [field: SerializeField] public float                PayloadDamageMultiplier   { get; private set; } = 0.5f;
        [Tooltip("Threshold value for HealthBelow and ResourceFull conditions (0–1).")]
        [field: SerializeField] public float                PayloadConditionThreshold { get; private set; } = 0.5f;

        protected override int MaxConnectors => 2;
    }
}