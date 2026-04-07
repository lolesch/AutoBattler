using System;
using System.Collections.Generic;
using Code.Data.Pawns;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.Modules.HexGrid
{
    /// <summary>
    /// A* pathfinder for the hex grid.
    /// </summary>
    public static class HexPathfinder
    {
        private const int OccupiedCostBonus = 8;

        /// <summary>
        /// Finds the shortest weighted path from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="from">Start hex.</param>
        /// <param name="to">Goal hex. Redirected to nearest valid neighbor if blocked.</param>
        /// <param name="invalidSet">Hexes that cannot be entered at all (impassable terrain, reserved destinations).</param>
        /// <param name="grid">Grid context used for terrain lookup.</param>
        /// <param name="occupiedSet">
        /// Hexes occupied by standing units. Passable during traversal at a high cost penalty,
        /// but invalid as a landing destination. Pass null for uniform traversal.
        /// </param>
        /// <param name="costMap">Per-terrain movement costs for the moving pawn. Null = uniform cost.</param>
        public static PathNode FindPath(
            Hex            from,
            Hex            to,
            HashSet<Hex>   invalidSet,
            IHexGrid       grid,
            HashSet<Hex>   occupiedSet = null,
            TerrainCostMap costMap     = null)
        {
            if (from.Equals(to))
                return new PathNode(from, null, 0f);

            // Occupied hexes are invalid as destinations but passable in traversal —
            // merge them into a combined set only for the destination resolution call.
            HashSet<Hex> destinationBlockedSet;
            if (occupiedSet != null && occupiedSet.Count > 0)
            {
                destinationBlockedSet = new HashSet<Hex>(invalidSet);
                destinationBlockedSet.UnionWith(occupiedSet);
            }
            else
            {
                destinationBlockedSet = invalidSet;
            }

            to = to.GetNearestValidPosition(destinationBlockedSet);

            if (to == Hex.Invalid || from == Hex.Invalid)
                return LogInvalidPath();

            var idealLine = to.Subtract(from);
            var openList  = new List<PathNode> { new PathNode(from, null, 0f) };
            var closedSet = new HashSet<Hex>();
            var costSoFar = new Dictionary<Hex, float> { { from, 0f } };

            while (openList.Count > 0)
            {
                var current = openList[0];
                openList.RemoveAt(0);

                if (!closedSet.Add(current.Hex))
                    continue;

                if (current.Hex.Equals(to))
                    return current;

                foreach (var neighbor in current.Hex.Neighbors())
                {
                    // Truly impassable — skip entirely.
                    if (closedSet.Contains(neighbor) || invalidSet.Contains(neighbor))
                        continue;

                    var terrainCost = grid != null ? GetTerrainCost(neighbor, grid, costMap) : 1;

                    // Occupied hexes are passable but expensive — pathfinder routes around
                    // clusters of units if a reasonable detour exists.
                    if (occupiedSet != null && occupiedSet.Contains(neighbor))
                        terrainCost += OccupiedCostBonus;

                    var newCost = costSoFar[current.Hex] + terrainCost;

                    if (costSoFar.TryGetValue(neighbor, out var existing) && newCost >= existing)
                        continue;

                    costSoFar[neighbor] = newCost;

                    var priority = newCost + Heuristic(neighbor, to) + Deviation(neighbor, current.Hex, idealLine);
                    var node     = new PathNode(neighbor, current, priority);
                    var insertAt = openList.BinarySearch(node);
                    openList.Insert(insertAt < 0 ? ~insertAt : insertAt, node);
                }
            }

            return LogInvalidPath();
        }

        private static int Heuristic(Hex from, Hex to) => from.Distance(to);

        private static float Deviation(Hex neighbor, Hex current, Hex idealLine)
        {
            var step  = neighbor.Subtract(current);
            var cross = Math.Abs(idealLine.q * step.r - idealLine.r * step.q);
            return cross * 0.001f;
        }

        private static int GetTerrainCost(Hex hex, IHexGrid grid, TerrainCostMap costMap)
        {
            if (costMap == null) return 1;
            var terrain = grid.GetTerrain(hex);
            return costMap.GetCost(terrain);
        }

        private static PathNode LogInvalidPath()
        {
            Debug.LogWarning("[HexPathfinder] No valid path found.");
            return null;
        }
    }
}