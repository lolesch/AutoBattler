using System;
using UnityEngine;
using Code.Data.Enums;
using UnityEngine.Serialization;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public struct PawnStatModifier : IPawnStatModifier
    {
        [field: SerializeField] public PawnStatType PawnStat { get; private set; }
        [field: SerializeField] public Modifier Modifier { get; private set; }

        public PawnStatModifier( PawnStatType pawnStat, Modifier modifier )
        {
            this.PawnStat = pawnStat;
            this.Modifier = modifier;
        }
    }

    internal interface IPawnStatModifier
    {
        PawnStatType PawnStat { get; }
        Modifier Modifier { get; }
    }
}