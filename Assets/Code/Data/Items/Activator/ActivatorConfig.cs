using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Activator
{
    [CreateAssetMenu(fileName = "ActivatorConfig", menuName = "Configs/Items/Activator")]
    public sealed class ActivatorConfig : ItemConfig
    {
        [Header("Activator Properties")]
        [field: SerializeField] public ActivatorType ActivatorType      { get; private set; }
        [Tooltip("Multiplier applied to weapon attack speed. Only used by ModifyCooldown.")]
        [field: SerializeField] public float         CooldownMultiplier { get; private set; } = 1f;

        protected override int MaxConnectors => 2;
    }
}