using System.Collections.Generic;
using Code.Runtime.Container;
using Code.Runtime.Container.Items;
using UnityEngine;

namespace Code.Runtime.GUI.Inventory
{
    /// <summary>
    /// Scene-view debug overlay for item chain connections.
    /// Draws connector slots and connection state for all items in the bound container.
    ///
    /// Per connector:
    ///   - Dot at the connector's slot cell (always)
    ///   - Yellow line between two dots when both sides agree to connect (valid connection)
    ///   - Red arrow outward when the connector has no matching neighbour (unconnected)
    ///
    /// Bind() must be called after InventoryView.RefreshView() whenever the active pawn changes.
    /// </summary>
    [RequireComponent(typeof(InventoryView))]
    public sealed class ChainOverlayView : MonoBehaviour
    {
        [SerializeField] private InventoryView _inventoryView;

        private ITetrisContainer                  _container;
        private IReadOnlyList<SlotView>           _slots;
        private HashSet<(Vector2Int, Vector2Int)> _validConnections = new();

        private static readonly Color ColorConnected   = new(1.00f, 0.85f, 0.00f, 1f); // yellow
        private static readonly Color ColorUnconnected = new(1.00f, 0.20f, 0.20f, 1f); // red
        private static readonly Color ColorDot         = Color.white;

        private const float DotRadius   = 4f;
        private const float ArrowLength = 0.6f; // fraction of cell size

        // ── Lifecycle ─────────────────────────────────────────────────────

        private void Awake() => _inventoryView ??= GetComponent<InventoryView>();

        // ── Public API ────────────────────────────────────────────────────

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

        // ── Internal ──────────────────────────────────────────────────────

        private void OnContentsChanged(IReadOnlyDictionary<Vector2Int, ITetrisItem> _)
        {
            _slots            = _inventoryView.Slots;
            _validConnections = FindValidConnections();
        }

        // ── Gizmos ────────────────────────────────────────────────────────

        private void OnDrawGizmos()
        {
            if (_container == null || _slots == null)
                return;

            var validConnections = _validConnections;

            foreach (var kvp in _container.Contents)
            {
                var item = kvp.Value;
                var pos  = kvp.Key;

                foreach (var (slotPos, direction) in item.GetGridConnectors(pos))
                {
                    var dotWorld = GetWorldPos(slotPos);
                    if (!dotWorld.HasValue)
                        continue;

                    Gizmos.color = ColorDot;
                    Gizmos.DrawSphere(dotWorld.Value, DotRadius);

                    var targetCell = slotPos + direction;
                    var key        = MakeKey(slotPos, targetCell);

                    if (validConnections.Contains(key))
                    {
                        if (!IsLowerSide(slotPos, targetCell))
                            continue;

                        var targetWorld = GetWorldPos(targetCell);
                        if (!targetWorld.HasValue)
                            continue;

                        Gizmos.color = ColorConnected;
                        Gizmos.DrawLine(dotWorld.Value, targetWorld.Value);
                    }
                    else
                    {
                        DrawArrow(dotWorld.Value, GetArrowTip(dotWorld.Value, direction));
                    }
                }
            }
        }

        // ── Connection resolution ─────────────────────────────────────────

        /// <summary>
        /// BFS from every weapon. Queue carries the arrival direction so we skip only
        /// the one connector pointing back the way we came — not all connectors sharing
        /// the same slotPos (which broke 1-cell items where all connectors share the anchor).
        /// </summary>
        private HashSet<(Vector2Int, Vector2Int)> FindValidConnections()
        {
            var result  = new HashSet<(Vector2Int, Vector2Int)>();
            var visited = new HashSet<ITetrisItem>();

            foreach (var kvp in _container.Contents)
            {
                if (kvp.Value is not IWeaponItem weapon)
                    continue;

                if (visited.Contains(weapon))
                    continue;

                visited.Add(weapon);

                // Queue: (item, anchor pos, direction we arrived FROM — zero for weapon start)
                var queue = new Queue<(ITetrisItem item, Vector2Int pos, Vector2Int arrivalDir)>();
                queue.Enqueue((weapon, kvp.Key, Vector2Int.zero));

                while (queue.Count > 0)
                {
                    var (current, currentPos, arrivalDir) = queue.Dequeue();

                    Debug.Log($"[Chain] Dequeued {current.Name} @ {currentPos} | arrivalDir:{arrivalDir}");

                    foreach (var (slotPos, direction) in current.GetGridConnectors(currentPos))
                    {
                        Debug.Log($"  connector slotPos:{slotPos} dir:{direction} | skipBack:{direction == -arrivalDir}");

                        // Skip the connector pointing back the way we came
                        if (direction == -arrivalDir)
                            continue;

                        var targetCell = slotPos + direction;

                        if (!_container.ContentPointer.TryGetValue(targetCell, out var neighbourOrigin))
                            continue;

                        if (!_container.Contents.TryGetValue(neighbourOrigin, out var neighbour))
                            continue;

                        if (visited.Contains(neighbour))
                            continue;

                        var matches = HasMatchingConnector(neighbour, neighbourOrigin, targetCell, -direction);
                        Debug.Log($"  → neighbour:{neighbour.Name} @ {neighbourOrigin} | HasMatchingConnector:{matches}");

                        if (!matches)
                            continue;

                        result.Add(MakeKey(slotPos, targetCell));
                        visited.Add(neighbour);
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

        // ── Arrow drawing ─────────────────────────────────────────────────

        private void DrawArrow(Vector3 from, Vector3 tip)
        {
            Gizmos.color = ColorUnconnected;
            Gizmos.DrawLine(from, tip);

            var shaft    = (tip - from).normalized;
            var right    = new Vector3(-shaft.y, shaft.x, 0f);
            var headSize = DotRadius * 2f;

            Gizmos.DrawLine(tip, tip - shaft * headSize + right * headSize * 0.5f);
            Gizmos.DrawLine(tip, tip - shaft * headSize - right * headSize * 0.5f);
        }

        // ── Coordinate helpers ────────────────────────────────────────────

        private Vector3? GetWorldPos(Vector2Int gridPos)
        {
            if (_container == null || _slots == null)
                return null;

            var index = gridPos.x + gridPos.y * _container.GridSize.x;
            if (index < 0 || index >= _slots.Count)
                return null;

            var rt = _slots[index].GetComponent<RectTransform>();
            return rt != null ? rt.position : (Vector3?)null;
        }

        /// <summary>
        /// Converts a grid-space direction to world-space by sampling adjacent slot positions.
        /// Grid Y increases downward visually — inverse of Unity world Y.
        /// </summary>
        private Vector3 GridDirToWorld(Vector2Int gridDir)
        {
            var o = GetWorldPos(new Vector2Int(0, 0));
            var r = GetWorldPos(new Vector2Int(1, 0));
            var d = GetWorldPos(new Vector2Int(0, 1));

            if (!o.HasValue)
                return Vector3.zero;

            var worldRight = r.HasValue ? (r.Value - o.Value).normalized : Vector3.right;
            var worldDown  = d.HasValue ? (d.Value - o.Value).normalized : Vector3.down;

            return gridDir.x * worldRight + gridDir.y * worldDown;
        }

        private Vector3 GetArrowTip(Vector3 dotWorld, Vector2Int gridDir)
        {
            var o = GetWorldPos(new Vector2Int(0, 0));
            var r = GetWorldPos(new Vector2Int(1, 0));
            var d = GetWorldPos(new Vector2Int(0, 1));

            float cellSize = 30f;
            if      (o.HasValue && r.HasValue) cellSize = Vector3.Distance(o.Value, r.Value);
            else if (o.HasValue && d.HasValue) cellSize = Vector3.Distance(o.Value, d.Value);

            return dotWorld + GridDirToWorld(gridDir) * cellSize * ArrowLength;
        }

        private static (Vector2Int, Vector2Int) MakeKey(Vector2Int a, Vector2Int b) =>
            IsLowerSide(a, b) ? (a, b) : (b, a);

        private static bool IsLowerSide(Vector2Int a, Vector2Int b) =>
            a.y < b.y || (a.y == b.y && a.x < b.x);
    }
}