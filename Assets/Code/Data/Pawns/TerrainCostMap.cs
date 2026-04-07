using System;
using System.Collections.Generic;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Pawns
{
    [Serializable]
    public sealed class TerrainCostMap
    {
        [Serializable]
        public struct Entry
        {
            public TerrainType terrain;
            [Min(1)] public int cost;
        }
 
        [SerializeField] private List<Entry> _entries = new();
        [SerializeField, Min(1)] private int _defaultCost = 1;
 
        public int GetCost(TerrainType terrain)
        {
            foreach (var e in _entries)
                if (e.terrain == terrain) return e.cost;
            return _defaultCost;
        }
    }
}