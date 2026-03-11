using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Code.Runtime.Inventory
{
    public sealed class ChainTopology
    {
        public IReadOnlyList<IItemChain>                                           Chains               { get; }
        public HashSet<(Vector2Int, Vector2Int)>                                   ConnectedEdges       { get; }
        // Downstream: (connectorSlotPos, connectorDirection) on the item further from the root.
        public IReadOnlyDictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>> DownstreamConnectors { get; }
        // Upstream: (connectorSlotPos, connectorDirection) on the item closer to the root.
        public IReadOnlyDictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>> UpstreamConnectors   { get; }

        public ChainTopology(
            IReadOnlyList<IItemChain>                                        chains,
            HashSet<(Vector2Int, Vector2Int)>                                connectedEdges,
            Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>       downstreamConnectors,
            Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>       upstreamConnectors)
        {
            Chains               = chains;
            ConnectedEdges       = connectedEdges;
            DownstreamConnectors = downstreamConnectors;
            UpstreamConnectors   = upstreamConnectors;
        }

        public static readonly ChainTopology Empty = new(
            System.Array.Empty<IItemChain>(),
            new HashSet<(Vector2Int, Vector2Int)>(),
            new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>(),
            new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>());
    }

    public static class ChainResolver
    {
        public static IReadOnlyList<IItemChain> Resolve(ITetrisContainer container)
            => ResolveTopology(container).Chains;

        public static ChainTopology ResolveTopology(ITetrisContainer container)
        {
            var allRoots = FindAllRoots(container);
            if (allRoots.Count == 0)
                return ChainTopology.Empty;

            var overriddenWeapons = FindOverriddenWeapons(container, allRoots);

            var chains               = new List<IItemChain>();
            var connectedEdges       = new HashSet<(Vector2Int, Vector2Int)>();
            var downstreamConnectors = new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>();
            var upstreamConnectors   = new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>();

            foreach (var (root, rootPos) in allRoots)
            {
                if (root is IWeaponItem w && overriddenWeapons.Contains(w))
                    continue;

                foreach (var (rootSlotPos, rootDirection) in root.GetGridConnectors(rootPos))
                {
                    var targetCell = rootSlotPos + rootDirection;

                    if (!container.ContentPointer.TryGetValue(targetCell, out var firstOrigin))
                        continue;

                    if (!container.Contents.TryGetValue(firstOrigin, out var firstNeighbour))
                        continue;

                    if (!HasMatchingConnector(firstNeighbour, firstOrigin, targetCell, -rootDirection))
                        continue;

                    connectedEdges.Add(MakeKey(rootSlotPos, targetCell));

                    // Root's outgoing connector is upstream.
                    MarkConnector(upstreamConnectors, root, rootSlotPos, rootDirection);

                    var modifiers = new List<ITetrisItem>();
                    var visited   = new HashSet<ITetrisItem> { root };
                    var queue     = new Queue<(ITetrisItem item, Vector2Int pos, Vector2Int inSlotPos, Vector2Int inDirection)>();
                    queue.Enqueue((firstNeighbour, firstOrigin, targetCell, -rootDirection));

                    while (queue.Count > 0)
                    {
                        var (current, currentPos, inSlotPos, inDirection) = queue.Dequeue();

                        if (visited.Contains(current))
                            continue;

                        visited.Add(current);
                        modifiers.Add(current);

                        // The connector this item was arrived at is downstream.
                        MarkConnector(downstreamConnectors, current, inSlotPos, inDirection);

                        foreach (var (slotPos, direction) in current.GetGridConnectors(currentPos))
                        {
                            var nextCell = slotPos + direction;

                            if (!container.ContentPointer.TryGetValue(nextCell, out var nextOrigin))
                                continue;

                            if (!container.Contents.TryGetValue(nextOrigin, out var next))
                                continue;

                            if (!HasMatchingConnector(next, nextOrigin, nextCell, -direction))
                                continue;

                            if (visited.Contains(next))
                                continue;

                            connectedEdges.Add(MakeKey(slotPos, nextCell));

                            // The outgoing connector on current toward next is upstream.
                            MarkConnector(upstreamConnectors, current, slotPos, direction);

                            queue.Enqueue((next, nextOrigin, nextCell, -direction));
                        }
                    }

                    var chain = new ItemChain(root, modifiers);
                    chains.Add(chain);
                    //LogChain(chain);
                    LogChainDetails(chain);
                }
            }

            return new ChainTopology(chains, connectedEdges, downstreamConnectors, upstreamConnectors);
        }
        
        private static void LogChain(IItemChain chain)
        {
            if (!chain.IsValid) return;

            var sb = new StringBuilder();
            sb.Append($"[Chain] {chain.Root.GetType().Name}({chain.Root.Name})");

            foreach (var item in chain.Modifiers)
                sb.Append($" → {item.GetType().Name}({item.Name})");

            Debug.Log(sb.ToString());
        }
        
        private static void LogChainDetails(IItemChain chain)
        {
            if (!chain.IsValid) return;

            var sb = new StringBuilder();
            sb.Append($"[Chain] {GetSemanticLabel(chain.Root, true)}({chain.Root.Name})");
            foreach (var item in chain.Modifiers)
                sb.Append($" → {GetSemanticLabel(item, false)}({item.Name})");
            
            Debug.Log(sb.ToString());
        }
        
        // Similar to PawnCombatController.GetSemanticLabel -> could live in a static helper class
        private static string GetSemanticLabel(ITetrisItem item, bool isRoot) => item switch
        {
            IWeaponItem    when isRoot  => "Weapon",
            IWeaponItem                 => "Payload",
            IAmplifierItem              => "Amplifier",
            IConverterItem              => "Converter",
            _ when isRoot               => "Trigger",
            _                           => item.GetType().Name,
        };

        private static void MarkConnector(
            Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>> dict,
            ITetrisItem item,
            Vector2Int  slotPos,
            Vector2Int  direction)
        {
            if (!dict.TryGetValue(item, out var set))
            {
                set = new HashSet<(Vector2Int, Vector2Int)>();
                dict[item] = set;
            }
            set.Add((slotPos, direction));
        }

        private static HashSet<IWeaponItem> FindOverriddenWeapons(
            ITetrisContainer                        container,
            List<(IChainRoot root, Vector2Int pos)> allRoots)
        {
            var overridden = new HashSet<IWeaponItem>();

            foreach (var (root, rootPos) in allRoots)
            {
                if (root is IWeaponItem)
                    continue;

                foreach (var (slotPos, direction) in root.GetGridConnectors(rootPos))
                {
                    var targetCell = slotPos + direction;

                    if (!container.ContentPointer.TryGetValue(targetCell, out var neighbourOrigin))
                        continue;

                    if (!container.Contents.TryGetValue(neighbourOrigin, out var neighbour))
                        continue;

                    if (neighbour is not IWeaponItem weapon)
                        continue;

                    if (!HasMatchingConnector(neighbour, neighbourOrigin, targetCell, -direction))
                        continue;

                    overridden.Add(weapon);
                }
            }

            return overridden;
        }

        private static List<(IChainRoot root, Vector2Int pos)> FindAllRoots(ITetrisContainer container)
        {
            var roots = new List<(IChainRoot, Vector2Int)>();
            foreach (var kvp in container.Contents)
                if (kvp.Value is IChainRoot root)
                    roots.Add((root, kvp.Key));
            return roots;
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

        private static (Vector2Int, Vector2Int) MakeKey(Vector2Int a, Vector2Int b) =>
            IsLowerSide(a, b) ? (a, b) : (b, a);

        private static bool IsLowerSide(Vector2Int a, Vector2Int b) =>
            a.y < b.y || (a.y == b.y && a.x < b.x);
    }
}