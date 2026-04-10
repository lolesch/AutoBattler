using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using Submodules.Utility.Extensions;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Runtime.Modules.HexGrid
{
    /// <summary>
    /// Scene-level spatial authority for the hex grid.
    /// Owns occupant registration, terrain lookup, and invalid-position queries.
    /// </summary>
    public sealed class HexGridController : MonoBehaviour, IHexGrid
    {
        //[SerializeField] private Grid    grid;
        [SerializeField] private Tilemap terrain;
        [SerializeField] private int levelIndex;
        
        //TODO: a wrapper holding all terrain tiles
        [SerializeField] private TerrainTileBase expr;

        public TerrainType GetTerrain(Hex hex)
        {
            var cell = hex.ToCell();
            return this.terrain.GetTile(cell) is TerrainTileBase terrain
                ? terrain.type
                : TerrainType.Impassable;
        }

        public void SetTerrain(Hex hex, TerrainType type)
        {
            var terrain = type switch
            {
                TerrainType.Dirt => expr,
                TerrainType.Sand => expr,
                TerrainType.Snow => expr,
                TerrainType.Impassable or
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            Debug.LogWarning($"[HexGrid] Set terrain at {hex} from {GetTerrain(hex)} to {terrain.type}", this);
            this.terrain.SetTile(hex.ToCell(), terrain);
        }

        [ContextMenu("SaveMap")]
        private void SaveMap()
        {
            var allTerrainTiles = terrain.GetAllCellTiles<TerrainTileBase>();
            
            var newTerrain = ScriptableObject.CreateInstance<TerrainData>();
            
            newTerrain.name = $"Terrain {levelIndex}";
            newTerrain.levelIndex = levelIndex;
            newTerrain.terrainTiles = allTerrainTiles.ToList();
            
            newTerrain.Save();
        }

        private void LoadMap()
        {
            throw new NotImplementedException();
        }

        private void ClearMap()
        {
            throw new NotImplementedException();
        }
    }
    
    public interface IHexGrid
    {
        /// <summary>Returns the terrain type at the given hex.</summary>
        TerrainType GetTerrain(Hex hex);
 
        /// <summary>Writes a terrain type onto the given hex tile.</summary>
        void SetTerrain(Hex hex, TerrainType type);
    }
}