using System;
using System.Collections.Generic;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Runtime.Modules.HexGrid
{
    [Serializable]
    public class TerrainTileBase : HexagonalRuleTile
    {
        [Header("Terrain")]
        [field: SerializeField] public TerrainType type { get; private set; }
        
        public List<TerrainModifier> modifiers;
    }

    [System.Serializable]
    public class TerrainModifier
    {
        public StatusEffect appliedStatus;
        public float intensity;
    }
    
    public class StatusEffect : ScriptableObject{}
}