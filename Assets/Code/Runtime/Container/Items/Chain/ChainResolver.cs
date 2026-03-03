using System.Collections.Generic;
using Code.Runtime.Container;
using Code.Runtime.Container.Items;
using UnityEngine;

namespace Code.Runtime.Container.Items.Chain
{
    /// <summary>
    /// Traverses the ITetrisContainer and builds one ItemChain per weapon found.
    ///
    /// Connection rule:
    ///   Item A connects to item B via a connector when:
    ///     1. A has a connector at gridSlotPos with gridDirection D
    ///     2. The target cell (gridSlotPos + D) maps to item B in ContentPointer
    ///     3. B has a connector whose slotPos equals the target cell
    ///        AND whose direction equals -D (pointing back toward A)
    ///
    /// This ensures both items intentionally agree to connect at that boundary.
    /// A single global visited set enforces each item belongs to at most one chain.
    /// </summary>
    public static class ChainResolver
    {
        public static IReadOnlyList<IItemChain> Resolve(ITetrisContainer container)
        {
            var weapons = FindAllWeapons(container);
            if (weapons.Count == 0)
                return System.Array.Empty<IItemChain>();

            var chains  = new List<IItemChain>(weapons.Count);
            var visited = new HashSet<ITetrisItem>();

            foreach (var (weapon, weaponPos) in weapons)
            {
                if (visited.Contains(weapon))
                    continue;

                visited.Add(weapon);

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

                        if (visited.Contains(neighbour))
                            continue;

                        if (!HasMatchingConnector(neighbour, neighbourOrigin, targetCell, -direction))
                            continue;

                        visited.Add(neighbour);
                        modifiers.Add(neighbour);
                        queue.Enqueue((neighbour, neighbourOrigin));
                    }
                }

                chains.Add(new ItemChain(weapon, modifiers));
            }

            return chains;
        }

        /// <summary>
        /// Returns true if the item has a connector sitting at expectedSlotPos
        /// pointing in expectedDirection.
        /// </summary>
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
    }
}