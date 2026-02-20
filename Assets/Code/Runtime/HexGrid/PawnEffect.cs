using System;
using Code.Runtime.HexGrid.HexGridInspector.Runtime;
using UnityEngine;

namespace Code.Runtime.HexGrid
{
    [Serializable]
    public sealed class PawnEffect
    {
        [SerializeField] public HexGridBool shape;
        [SerializeField] public string effect;
    }
}