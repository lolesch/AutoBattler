using System.Collections.Generic;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.Modules.HexGrid
{
    public sealed class TerrainData : ScriptableObject
    {
        [SerializeField] public int levelIndex;
        [SerializeField] public List<CellTile<TerrainTileBase>> terrainTiles;
    }
}