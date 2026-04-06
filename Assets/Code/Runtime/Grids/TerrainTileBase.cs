using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.Grids
{
    [Serializable]
    public class TerrainTileBase : HexagonalRuleTile
    {
        [Header("Terrain")]
        [field: SerializeField] public TerrainType type { get; private set; }
        
        public List<TerrainModifier> modifiers;
    }
    
    public enum TerrainType
    {
        Normal,
        Burning,
        Frozen,
        Toxic,
        Conductive,
        Obstructed
    }
    
    [System.Serializable]
    public class TerrainModifier
    {
        public StatusEffect appliedStatus;
        public float intensity;
    }
    
    public class StatusEffect : ScriptableObject{}
}