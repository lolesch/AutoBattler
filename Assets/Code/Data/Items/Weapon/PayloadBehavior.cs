using System;
using System.Collections.Generic;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Weapon
{
    [Serializable]
    public sealed class PayloadBehavior
    {
        [field: SerializeField] public ConditionType    Condition          { get; private set; }
        [field: SerializeField] public float            ConditionThreshold { get; private set; } = 0.5f;
        [field: SerializeField] public PayloadTargeting Targeting          { get; private set; }
        [field: SerializeField] public int              Range              { get; private set; } = 1;
        [field: SerializeField] public PayloadTiming    Timing             { get; private set; }
        [field: SerializeField] public float            TimingValue        { get; private set; }

        [SerializeReference]
        private List<PayloadEffect> _effects = new();
        public IReadOnlyList<PayloadEffect> Effects => _effects;
    }

    public enum PayloadTargeting
    {
        Single,  // Target only
        Aoe,     // Hex range around target
        Line,    // Path from caster to target
        Self,    // Origin pawn's hex
    }

    public enum PayloadTiming
    {
        Instant,
        Delayed,
        Repeating,
    }
}
