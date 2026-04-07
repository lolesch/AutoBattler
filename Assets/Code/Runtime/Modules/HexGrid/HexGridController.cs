using System;
using System.Collections.Generic;
using Code.Data.Enums;
using Submodules.Utility.Extensions;
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
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private Grid    grid;
        
        //TODO: a wrapper holding all terrain tiles
        [SerializeField] private TerrainTileBase expr;

        public TerrainType GetTerrain(Hex hex)
        {
            var cell = hex.ToCell();
            return tilemap.GetTile(cell) is TerrainTileBase terrain
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
            tilemap.SetTile(hex.ToCell(), terrain);
        }
    }
    
    public interface IHexGrid
    {
        /// <summary>Returns the terrain type at the given hex.</summary>
        TerrainType GetTerrain(Hex hex);
 
        /// <summary>Writes a terrain type onto the given hex tile.</summary>
        void SetTerrain(Hex hex, TerrainType type);
    }
    
    public interface IHexOccupant
    {
        Hex HexPosition { get; }
        void MoveTo(Hex hex); // does this belong here or into the IPawn?
    }
}