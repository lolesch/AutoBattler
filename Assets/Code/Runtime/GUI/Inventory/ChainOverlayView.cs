using System.Collections.Generic;
using Code.Runtime.Container;
using Code.Runtime.Container.Items;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.GUI.Inventory
{
    [RequireComponent(typeof(InventoryView))]
    public sealed class ChainOverlayView : MonoBehaviour
    {
        [SerializeField] private InventoryView _inventoryView;

        private ITetrisContainer                  _container;
        private IReadOnlyList<ISlotView>           _slots;
        private HashSet<(Vector2Int, Vector2Int)> _validConnections = new();

        private static readonly Color ColorConnected   = new(1.00f, 0.85f, 0.00f, 1f);
        private static readonly Color ColorUnconnected = new(1.00f, 0.20f, 0.20f, 1f);
        private static readonly Color ColorDot         = Color.white;

        private const float DotRadius   = 4f;
        private const float ArrowLength = 0.6f;

        public void Bind(ITetrisContainer container)
        {
            if (_container != null)
                _container.OnContentsChanged -= OnContentsChanged;

            _container = container;
            _slots     = _inventoryView.Slots;

            if (_container != null)
                _container.OnContentsChanged += OnContentsChanged;

            _validConnections = FindValidConnections();
        }

        private void OnContentsChanged(IReadOnlyDictionary<Vector2Int, ITetrisItem> _)
        {
            _slots            = _inventoryView.Slots;
            _validConnections = FindValidConnections();
        }

        private void OnDrawGizmos()
        {
            if (_container == null || _slots == null) return;

            // Measure world-space cell size and axis directions from actual slot positions.
            // GetWorldPos is called once here while positions are guaranteed correct.
            // GetArrowTip previously re-called GetWorldPos and received stale values — removed.
            var o = GetWorldPos(new Vector2Int(0, 0));
            var r = GetWorldPos(new Vector2Int(1, 0));
            var d = GetWorldPos(new Vector2Int(0, 1));

            if (!o.HasValue) return;

            var cellSize   = r.HasValue ? Vector2.Distance(o.Value, r.Value)
                           : d.HasValue ? Vector2.Distance(o.Value, d.Value)
                           : 30f;
            var worldRight = r.HasValue ? (r.Value - o.Value).normalized : Vector2.right;
            var worldDown  = d.HasValue ? (d.Value - o.Value).normalized : Vector2.down;

            foreach (var kvp in _container.Contents)
            {
                var item = kvp.Value;
                var pos  = kvp.Key;

                foreach (var (slotPos, direction) in item.GetGridConnectors(pos))
                {
                    var dotWorld = GetWorldPos(slotPos);
                    if (!dotWorld.HasValue) continue;

                    Gizmos.color = ColorDot;
                    Gizmos.DrawSphere(dotWorld.Value, DotRadius);

                    var targetCell = slotPos + direction;
                    var key        = MakeKey(slotPos, targetCell);

                    if (_validConnections.Contains(key))
                    {
                        if (!IsLowerSide(slotPos, targetCell)) continue;

                        var targetWorld = GetWorldPos(targetCell);
                        if (!targetWorld.HasValue) continue;

                        Gizmos.color = ColorConnected;
                        Gizmos.DrawLine(dotWorld.Value, targetWorld.Value);
                    }
                    else
                    {
                        Gizmos.color = ColorUnconnected;
                        Vector2 dir = (direction.x * worldRight + direction.y * worldDown) * cellSize * ArrowLength;
                        GizmosExtensions.DrawArrow2D(dotWorld.Value, dir, DotRadius * 2f);
                    }
                }
            }
        }

        private HashSet<(Vector2Int, Vector2Int)> FindValidConnections()
        {
            if (_container == null)
                return new HashSet<(Vector2Int, Vector2Int)>();

            var result  = new HashSet<(Vector2Int, Vector2Int)>();
            var visited = new HashSet<Vector2Int>();

            foreach (var kvp in _container.Contents)
            {
                if (kvp.Value is not IWeaponItem)
                    continue;

                var weaponAnchor = kvp.Key;

                if (visited.Contains(weaponAnchor))
                    continue;

                visited.Add(weaponAnchor);

                var queue = new Queue<(ITetrisItem item, Vector2Int pos, Vector2Int arrivalDir)>();
                queue.Enqueue((kvp.Value, weaponAnchor, Vector2Int.zero));

                while (queue.Count > 0)
                {
                    var (current, currentPos, arrivalDir) = queue.Dequeue();

                    foreach (var (slotPos, direction) in current.GetGridConnectors(currentPos))
                    {
                        if (direction == -arrivalDir)
                            continue;

                        var targetCell = slotPos + direction;

                        if (!_container.ContentPointer.TryGetValue(targetCell, out var neighbourOrigin))
                            continue;

                        if (!_container.Contents.TryGetValue(neighbourOrigin, out var neighbour))
                            continue;

                        if (visited.Contains(neighbourOrigin))
                            continue;

                        if (!HasMatchingConnector(neighbour, neighbourOrigin, targetCell, -direction))
                            continue;

                        result.Add(MakeKey(slotPos, targetCell));
                        visited.Add(neighbourOrigin);
                        queue.Enqueue((neighbour, neighbourOrigin, direction));
                    }
                }
            }

            return result;
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

        // Returns screen-space position as Vector2 — Z from rt.position is meaningless for UI overlays.
        private Vector2? GetWorldPos(Vector2Int gridPos)
        {
            if (_container == null || _slots == null) return null;

            var index = gridPos.x + gridPos.y * _container.GridSize.x;
            if (index < 0 || index >= _slots.Count) return null;

            var rt = (_slots[index] as SlotView)?.GetComponent<RectTransform>();
            return rt != null ? (Vector2)rt.position : (Vector2?)null;
        }

        private static (Vector2Int, Vector2Int) MakeKey(Vector2Int a, Vector2Int b) =>
            IsLowerSide(a, b) ? (a, b) : (b, a);

        private static bool IsLowerSide(Vector2Int a, Vector2Int b) =>
            a.y < b.y || (a.y == b.y && a.x < b.x);
    }
}