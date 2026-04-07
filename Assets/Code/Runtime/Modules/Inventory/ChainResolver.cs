using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.Runtime.Modules.Inventory
{
    public sealed class ChainTopology
    {
        public IReadOnlyList<IItemChain>                                           Chains               { get; }
        public HashSet<(Vector2Int, Vector2Int)>                                   ConnectedEdges       { get; }
        public IReadOnlyDictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>> DownstreamConnectors { get; }
        public IReadOnlyDictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>> UpstreamConnectors   { get; }
        /// <summary>Items resolved as chain roots by first-pass BFS position, not interface marker.</summary>
        public IReadOnlyCollection<ITetrisItem>                                    Roots                { get; }

        public ChainTopology(
            IReadOnlyList<IItemChain>                                        chains,
            HashSet<(Vector2Int, Vector2Int)>                                connectedEdges,
            Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>       downstreamConnectors,
            Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>       upstreamConnectors,
            HashSet<ITetrisItem>                                             roots)
        {
            Chains               = chains;
            ConnectedEdges       = connectedEdges;
            DownstreamConnectors = downstreamConnectors;
            UpstreamConnectors   = upstreamConnectors;
            Roots                = roots;
        }

        public static readonly ChainTopology Empty = new(
            System.Array.Empty<IItemChain>(),
            new HashSet<(Vector2Int, Vector2Int)>(),
            new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>(),
            new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>(),
            new HashSet<ITetrisItem>());
    }

    public static class ChainResolver
    {
        public static IReadOnlyList<IItemChain> Resolve(ITetrisContainer container)
            => ResolveTopology(container).Chains;

        public static ChainTopology ResolveTopology(ITetrisContainer container)
        {
            var adjacency     = BuildAdjacency(container);
            var resolvedRoots = ResolveRoots(container, adjacency);

            if (resolvedRoots.Count == 0)
                return ChainTopology.Empty;

            var chains               = new List<IItemChain>();
            var connectedEdges       = new HashSet<(Vector2Int, Vector2Int)>();
            var downstreamConnectors = new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>();
            var upstreamConnectors   = new Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>>();
            var rootSet              = new HashSet<ITetrisItem>(resolvedRoots.Select(r => r.root));

            foreach (var (root, rootPos) in resolvedRoots)
            {
                foreach (var (rootSlotPos, rootDirection) in root.GetGridConnectors(rootPos))
                {
                    if (!TryGetValidNeighbour(adjacency, container, root, rootSlotPos, rootDirection,
                            out var firstNeighbour, out var firstOrigin))
                        continue;

                    connectedEdges.Add(MakeKey(rootSlotPos, rootSlotPos + rootDirection));
                    MarkConnector(upstreamConnectors, root, rootSlotPos, rootDirection);

                    var modifiers = new List<ITetrisItem>();
                    var visited   = new HashSet<ITetrisItem> { root };
                    var queue     = new Queue<(ITetrisItem item, Vector2Int pos, Vector2Int inSlotPos, Vector2Int inDirection)>();
                    queue.Enqueue((firstNeighbour, firstOrigin, rootSlotPos + rootDirection, -rootDirection));

                    while (queue.Count > 0)
                    {
                        var (current, currentPos, inSlotPos, inDirection) = queue.Dequeue();
                        if (visited.Contains(current)) continue;
                        visited.Add(current);
                        modifiers.Add(current);

                        MarkConnector(downstreamConnectors, current, inSlotPos, inDirection);

                        foreach (var (slotPos, direction) in current.GetGridConnectors(currentPos))
                        {
                            if (!TryGetValidNeighbour(adjacency, container, current, slotPos, direction,
                                    out var next, out var nextOrigin)) continue;
                            if (visited.Contains(next)) continue;
                            if (IsTrigger(next) && !IsTrigger(current)) continue;

                            connectedEdges.Add(MakeKey(slotPos, slotPos + direction));
                            MarkConnector(upstreamConnectors, current, slotPos, direction);
                            queue.Enqueue((next, nextOrigin, slotPos + direction, -direction));
                        }
                    }

                    var hasWeapon = root is IWeaponItem || modifiers.Exists(m => m is IWeaponItem);
                    if (!hasWeapon)
                    {
                        Debug.LogWarning($"[ChainResolver] Root '{root.Name}' has no weapon in chain — skipped.");
                        continue;
                    }
                    
                    var chain = new ItemChain(root, modifiers);
                    chains.Add(chain);
                    //LogChain(chain);
                }
                
                if (root is IWeaponItem && !chains.Any(c => c.Root == root))
                    chains.Add(new ItemChain(root, new List<ITetrisItem>()));
            }

            return new ChainTopology(chains, connectedEdges, downstreamConnectors, upstreamConnectors, rootSet);
        }

        // ── First pass ────────────────────────────────────────────────────

        private static List<(ITetrisItem root, Vector2Int pos)> ResolveRoots(
            ITetrisContainer                           container,
            Dictionary<ITetrisItem, List<ITetrisItem>> adjacency)
        {
            var positionOf    = container.Contents.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            var assignedRoots = new HashSet<ITetrisItem>();
            var roots         = new List<(ITetrisItem, Vector2Int)>();

            foreach (var kvp in container.Contents)
            {
                if (kvp.Value is not IWeaponItem weapon) continue;

                var visited = new HashSet<ITetrisItem> { weapon };
                var queue   = new Queue<(ITetrisItem item, int depth)>();

                var weaponPos = kvp.Key;
                if (adjacency.TryGetValue(weapon, out var weaponNeighbors))
                    foreach (var neighbor in weaponNeighbors)
                        if (IsTrigger(neighbor) && IsUpstreamOf(neighbor, weapon, weaponPos, container))
                            queue.Enqueue((neighbor, 1));

                ITetrisItem furthestTrigger = null;
                var         maxDepth        = 0;

                while (queue.Count > 0)
                {
                    var (current, depth) = queue.Dequeue();
                    if (visited.Contains(current)) continue;
                    visited.Add(current);

                    if (depth > maxDepth) { maxDepth = depth; furthestTrigger = current; }

                    if (!adjacency.TryGetValue(current, out var neighbors)) continue;
                    foreach (var neighbor in neighbors)
                        if (IsTrigger(neighbor) && !visited.Contains(neighbor))
                            queue.Enqueue((neighbor, depth + 1));
                }

                var resolvedRoot = furthestTrigger ?? weapon;
                if (assignedRoots.Add(resolvedRoot))
                    roots.Add((resolvedRoot, positionOf[resolvedRoot]));
            }

            return roots;
        }
        
        private static bool IsUpstreamOf(
            ITetrisItem      trigger,
            ITetrisItem      weapon,
            Vector2Int       weaponOrigin,
            ITetrisContainer container)
        {
            var triggerOrigin = container.Contents.First(kvp => kvp.Value == trigger).Key;
            var weaponCells   = new HashSet<Vector2Int>(weapon.GetPointers(weaponOrigin));

            foreach (var (slotPos, direction) in trigger.GetGridConnectors(triggerOrigin))
                if (weaponCells.Contains(slotPos + direction))
                    return true;

            return false;
        }

        /// <summary>
        /// Builds an undirected connection graph filtered by connection validity rules.
        /// Only valid bidirectional connector pairs produce edges.
        /// </summary>
        private static Dictionary<ITetrisItem, List<ITetrisItem>> BuildAdjacency(ITetrisContainer container)
        {
            var adj = new Dictionary<ITetrisItem, List<ITetrisItem>>();

            foreach (var item in container.Contents.Values)
                adj[item] = new List<ITetrisItem>();

            foreach (var (pos, item) in container.Contents)
            {
                foreach (var (slotPos, direction) in item.GetGridConnectors(pos))
                {
                    var targetCell = slotPos + direction;
                    if (!container.ContentPointer.TryGetValue(targetCell, out var neighborOrigin)) continue;
                    if (!container.Contents.TryGetValue(neighborOrigin, out var neighbor)) continue;
                    if (!HasMatchingConnector(neighbor, neighborOrigin, targetCell, -direction)) continue;
                    if (!IsValidConnection(item, neighbor)) continue;
                    if (adj[item].Contains(neighbor)) continue;

                    adj[item].Add(neighbor);
                    adj[neighbor].Add(item);
                }
            }

            return adj;
        }
        
        // ── Logging ───────────────────────────────────────────────────────

        public static void LogChain(IItemChain chain)
        {
            if (!chain.IsValid) return;
            Debug.Log(FormatDetailed(chain));
        }
        
        private static string FormatDetailed(IItemChain chain)
        {
            var weapon = chain.Weapon;
            var sb     = new System.Text.StringBuilder("[Chain]");
            sb.Append($" {GetSemanticLabel(chain.Root, true)}({chain.Root.Name})");
            if (weapon != null)
                sb.Append($" dmg:{(float)weapon.Damage:F1} spd:{(float)weapon.AttackSpeed:F1} cost:{(float)weapon.ResourceCost:F1}");
            foreach (var item in chain.Modifiers)
            {
                var isPayload = item is IWeaponItem w && w != weapon;
                sb.Append($" → {GetSemanticLabel(item, false, isPayload)}({item.Name}");
                if (isPayload)
                    sb.Append($"|{((IWeaponItem)item).Payload.Condition}");
                sb.Append(")");
            }
            return sb.ToString();
        }

        private static string GetSemanticLabel(ITetrisItem item, bool isRoot, bool isPayload = false) => item switch
        {
            IWeaponItem    when isRoot    => "Weapon",
            IWeaponItem    when isPayload => "Payload",
            IWeaponItem                   => "Weapon",
            IAmplifierItem             => "Amplifier",
            IConverterItem             => "Converter",
            _              when isRoot => "Trigger",
            _                          => item.GetType().Name,
        };
        
        // ── Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if slotPos+direction points to a neighbour that is both
        /// connector-matched and present in the validated adjacency graph.
        /// Single check point used by both root→first and BFS inner loop.
        /// </summary>
        private static bool TryGetValidNeighbour(
            Dictionary<ITetrisItem, List<ITetrisItem>> adjacency,
            ITetrisContainer                           container,
            ITetrisItem                                from,
            Vector2Int                                 slotPos,
            Vector2Int                                 direction,
            out ITetrisItem                            neighbour,
            out Vector2Int                             neighbourOrigin)
        {
            neighbour       = null;
            neighbourOrigin = default;

            var targetCell = slotPos + direction;
            if (!container.ContentPointer.TryGetValue(targetCell, out neighbourOrigin)) return false;
            if (!container.Contents.TryGetValue(neighbourOrigin, out neighbour)) return false;
            if (!HasMatchingConnector(neighbour, neighbourOrigin, targetCell, -direction)) return false;
            if (!adjacency.TryGetValue(from, out var validNeighbors)) return false;
            return validNeighbors.Contains(neighbour);
        }

        private static bool IsValidConnection(ITetrisItem a, ITetrisItem b) => (a, b) switch
        {
            (IReactorItem,   IReactorItem)   => false,
            (IReactorItem,   IAmplifierItem) => false,
            (IReactorItem,   IConverterItem) => false,
            (IAmplifierItem, IReactorItem)   => false,
            (IConverterItem, IReactorItem)   => false,
            (IShifterItem, IAmplifierItem) => false,
            (IShifterItem, IConverterItem) => false,
            (IAmplifierItem, IShifterItem) => false,
            (IConverterItem, IShifterItem) => false,
            _                                => true,
        };

        private static bool IsTrigger(ITetrisItem item) => item is IShifterItem or IReactorItem;

        private static bool HasMatchingConnector(
            ITetrisItem item, Vector2Int placement,
            Vector2Int  expectedSlotPos, Vector2Int expectedDirection)
        {
            foreach (var (slotPos, direction) in item.GetGridConnectors(placement))
                if (slotPos == expectedSlotPos && direction == expectedDirection)
                    return true;
            return false;
        }

        private static void MarkConnector(
            Dictionary<ITetrisItem, HashSet<(Vector2Int, Vector2Int)>> dict,
            ITetrisItem item, Vector2Int slotPos, Vector2Int direction)
        {
            if (!dict.TryGetValue(item, out var set))
            {
                set = new HashSet<(Vector2Int, Vector2Int)>();
                dict[item] = set;
            }
            set.Add((slotPos, direction));
        }

        private static (Vector2Int, Vector2Int) MakeKey(Vector2Int a, Vector2Int b) =>
            IsLowerSide(a, b) ? (a, b) : (b, a);

        private static bool IsLowerSide(Vector2Int a, Vector2Int b) =>
            a.y < b.y || (a.y == b.y && a.x < b.x);
    }
}