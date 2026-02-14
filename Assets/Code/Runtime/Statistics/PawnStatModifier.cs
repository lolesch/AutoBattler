using System;
using UnityEngine;
using Code.Data.Enums;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public struct PawnStatModifier : IStatModifier
    {
        /// where to put this?
        //[field: SerializeField] protected float randomRoll { get; } = Random.value;
        //[SerializeField] protected Vector2 _range;
        
        [field: SerializeField] public StatType stat { get; private set; }
        [field: SerializeField] public Modifier modifier { get; private set; }

        public PawnStatModifier( StatType stat, Modifier modifier )
        {
            this.stat = stat;
            this.modifier = modifier;
        }
    }

    internal interface IStatModifier
    {
        StatType stat { get; }
        Modifier modifier { get; }
    }
}