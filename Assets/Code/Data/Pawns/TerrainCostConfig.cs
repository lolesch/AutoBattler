using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Pawns
{
    [Serializable]
    public sealed class TerrainCostConfig
    {
        [Serializable]
        public struct TerrainCost
        {
            public TerrainType terrain;
            [Min(1)] public int cost;
        }
 
        [SerializeField] private List<TerrainCost> map = new();
        [SerializeField, Min(1)] private int _defaultCost = 1;
 
        public int GetCost(TerrainType terrain)
        {
            foreach (var terrainCost in map.Where(terrainCost => terrain == terrainCost.terrain))
                return terrainCost.cost;
            
            return _defaultCost;
        }
    }
}