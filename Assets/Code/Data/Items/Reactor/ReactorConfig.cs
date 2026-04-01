using Code.Data.Enums;
using Code.Data.Items.Shifter;
using UnityEngine;

namespace Code.Data.Items.Reactor
{
    /// <summary>
    /// Unchained: applies a standalone pawn stat modifier (from StatItemConfig base).
    /// Chained: replaces the weapon's timer with event-based firing.
    /// Condition is checked each time the event fires — if not met, the chain does not fire.
    /// </summary>
    [CreateAssetMenu(fileName = "ReactorConfig", menuName = "Configs/Items/Reactor")]
    public sealed class ReactorConfig : StatItemConfig
    {
        [field: Header("Chain — Event")]
        [field: SerializeField] public ReactorType             ReactorType        { get; private set; }

        //TODO: rework conditions!
        [field: Header("Chain — Condition")]
        [field: SerializeField] public ConditionType  ConditionType      { get; private set; }
        [field: SerializeField] public float                   ConditionThreshold { get; private set; }

        public override int MaxConnectors => 1;
    }
}