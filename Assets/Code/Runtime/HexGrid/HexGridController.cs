using System;
using System.Collections.Generic;
using Code.Data.Enums;
using Submodules.Utility.Extensions;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Runtime.HexGrid
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

        private readonly List<IHexOccupant> _occupants = new();

        public void Register(IHexOccupant occupant)
        {
            if (!_occupants.Contains(occupant))
                _occupants.Add(occupant);
        }

        public void Unregister(IHexOccupant occupant) => _occupants.Remove(occupant);

        public IEnumerable<IHexOccupant>GetOccupantsInRange(Hex center, int range, PawnTeam? filter = null)
        {
            var hexesInRange = new HashSet<Hex>(center.HexRange(range));
            foreach (var occupant in _occupants)
            {
                if (!hexesInRange.Contains(occupant.HexPosition)) continue;
                if (filter.HasValue && occupant.Team != filter.Value) continue;
                yield return occupant;
            }
        }

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
        
        /*public HashSet<Hex> GetInvalidPositions(bool includeOccupied = false)
        {
            var invalid = new HashSet<Hex>(); // define static list of invalid positions per level and add that here
            if (!includeOccupied) return invalid;
            foreach (var o in _occupants)
                invalid.Add(o.HexPosition);
            return invalid;
        }*/
    }
    
    public interface IHexGrid
    {
        /// <summary>Returns all occupants within hex range of center, optionally filtered by team.</summary>
        IEnumerable<IHexOccupant> GetOccupantsInRange(Hex center, int range, PawnTeam? filter = null);
 
        /// <summary>Returns the terrain type at the given hex.</summary>
        TerrainType GetTerrain(Hex hex);
 
        /// <summary>Writes a terrain type onto the given hex tile.</summary>
        void SetTerrain(Hex hex, TerrainType type);
    }
}