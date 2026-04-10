using System;
using System.Collections.Generic;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Runtime.Modules.HexGrid
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Terrain Rule Tile", menuName = "2D/Tiles/Terrain Tile")]
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