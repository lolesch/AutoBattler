using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.Inventory
{
    public sealed class ChainTopology
    {
        public IReadOnlyList<IItemChain>                                           Chains               { get; }
        public HashSet<(Vector2Int, Vector2Int)>                                   ConnectedEdges       { get; }
        // Keyed by (connectorSlotPos, connectorDirection) on the downstream item.
        public IReadOnlyDictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>> DownstreamConnectors { get; }

        public ChainTopology(
            IReadOnlyList<IItemChain>                                        chains,
            HashSet<(Vector2Int, Vector2Int)>                                connectedEdges,
            Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>       downstreamConnectors)
        {
            Chains               = chains;
            ConnectedEdges       = connectedEdges;
            DownstreamConnectors = downstreamConnectors;
        }

        public static readonly ChainTopology Empty = new(
            System.Array.Empty<IItemChain>(),
            new HashSet<(Vector2Int, Vector2Int)>(),
            new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>());
    }

    public static class ChainResolver
    {
        public static IReadOnlyList<IItemChain> Resolve(ITetrisContainer container)
            => ResolveTopology(container).Chains;

        public static ChainTopology ResolveTopology(ITetrisContainer container)
        {
            var weapons = FindAllWeapons(container);
            if (weapons.Count == 0)
                return ChainTopology.Empty;

            // ── Phase 1: chain resolution ──────────────────────────────────
            // Global node-visited set — each item belongs to at most one chain.
            // Builds connectedEdges as a side effect.

            var chains         = new List<IItemChain>(weapons.Count);
            var visitedGlobal  = new HashSet<ITetrisItem>();
            var connectedEdges = new HashSet<(Vector2Int, Vector2Int)>();

            foreach (var (weapon, _) in weapons)
                visitedGlobal.Add(weapon);

            foreach (var (weapon, weaponPos) in weapons)
            {
                var modifiers = new List<ITetrisItem>();
                var queue     = new Queue<(ITetrisItem item, Vector2Int pos)>();
                queue.Enqueue((weapon, weaponPos));

                while (queue.Count > 0)
                {
                    var (current, currentPos) = queue.Dequeue();

                    foreach (var (slotPos, direction) in current.GetGridConnectors(currentPos))
                    {
                        var targetCell = slotPos + direction;

                        if (!container.ContentPointer.TryGetValue(targetCell, out var neighbourOrigin))
                            continue;

                        if (!container.Contents.TryGetValue(neighbourOrigin, out var neighbour))
                            continue;

                        if (!HasMatchingConnector(neighbour, neighbourOrigin, targetCell, -direction))
                            continue;

                        connectedEdges.Add(MakeKey(slotPos, targetCell));

                        if (visitedGlobal.Contains(neighbour))
                            continue;

                        visitedGlobal.Add(neighbour);
                        modifiers.Add(neighbour);
                        queue.Enqueue((neighbour, neighbourOrigin));
                    }
                }

                chains.Add(new ItemChain(weapon, modifiers));
            }

            // ── Phase 2: downstream marking ────────────────────────────────
            // One BFS per weapon. Uses directed-edge visited set instead of node
            // visited set. This allows a node to be reached from multiple directions
            // so all its incoming connectors get marked — critical for cycles.
            // Each directed edge (source→target) is only traversed once per pass,
            // which is sufficient to prevent infinite loops in any graph topology.
            // Only the root weapon of each pass is never marked downstream —
            // all other items including payload weapons are valid downstream targets.

            var downstreamConnectors = new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>();

            foreach (var (weapon, weaponPos) in weapons)
            {
                // Directed edge key: (sourceItem, targetItem)
                var visitedEdges = new HashSet<(ITetrisItem, ITetrisItem)>();
                var queue        = new Queue<(ITetrisItem item, Vector2Int pos)>();
                queue.Enqueue((weapon, weaponPos));

                while (queue.Count > 0)
                {
                    var (current, currentPos) = queue.Dequeue();

                    foreach (var (slotPos, direction) in current.GetGridConnectors(currentPos))
                    {
                        var targetCell = slotPos + direction;

                        if (!connectedEdges.Contains(MakeKey(slotPos, targetCell)))
                            continue;

                        if (!container.ContentPointer.TryGetValue(targetCell, out var neighbourOrigin))
                            continue;

                        if (!container.Contents.TryGetValue(neighbourOrigin, out var neighbour))
                            continue;

                        // Skip if this directed edge has already been traversed this pass.
                        if (!visitedEdges.Add((current, neighbour)))
                            continue;

                        // Root weapon of this pass is never downstream.
                        if (neighbour != weapon)
                        {
                            if (!downstreamConnectors.TryGetValue(neighbour, out var set))
                            {
                                set = new HashSet<(Vector2Int, Vector2Int)>();
                                downstreamConnectors[neighbour] = set;
                            }
                            set.Add((targetCell, -direction));
                        }

                        queue.Enqueue((neighbour, neighbourOrigin));
                    }
                }
            }

            return new ChainTopology(chains, connectedEdges, downstreamConnectors);
        }

        private static bool HasMatchingConnector(
            ITetrisItem item,
            Vector2Int  placement,
            Vector2Int  expectedSlotPos,
            Vector2Int  expectedDirection)
        {
            foreach (var (slotPos, direction) in item.GetGridConnectors(placement))
                if (slotPos == expectedSlotPos && direction == expectedDirection)
                    return true;

            return false;
        }

        private static List<(IWeaponItem weapon, Vector2Int pos)> FindAllWeapons(ITetrisContainer container)
        {
            var weapons = new List<(IWeaponItem, Vector2Int)>();
            foreach (var kvp in container.Contents)
                if (kvp.Value is IWeaponItem weapon)
                    weapons.Add((weapon, kvp.Key));
            return weapons;
        }

        private static (Vector2Int, Vector2Int) MakeKey(Vector2Int a, Vector2Int b) =>
            IsLowerSide(a, b) ? (a, b) : (b, a);

        private static bool IsLowerSide(Vector2Int a, Vector2Int b) =>
            a.y < b.y || (a.y == b.y && a.x < b.x);
    }
}