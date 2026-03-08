using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.GUI.Inventory
{
    public sealed class InventoryView : MonoBehaviour, IInventoryView
    {
        [SerializeField] private SlotView                _slotPrefab;
        [SerializeField] private GridLayoutGroup         _grid;
        [SerializeField] private ChainOverlayView        _chainOverlay;
        [SerializeField] private InventoryDragController _dragController;

        [SerializeField, ReadOnly, AllowNesting] private SlotView[] _slots;

        public IReadOnlyList<ISlotView> Slots => _slots.Cast<ISlotView>().ToList();

        private IPawn      _pawn;
        private Vector2Int _builtForSize;

        private void OnDestroy()
        {
            if (_pawn != null)
                _pawn.Inventory.OnContentsChanged -= OnContentsChanged;
        }

        public void RefreshView(IPawn pawn)
        {
            if (_pawn != pawn)
            {
                if (_pawn != null)
                    _pawn.Inventory.OnContentsChanged -= OnContentsChanged;

                _pawn = pawn;
                _pawn.Inventory.OnContentsChanged += OnContentsChanged;
            }

            RebuildSlotsIfNeeded(_pawn.Inventory.GridSize);

            _dragController?.Bind(_pawn.Inventory, Slots);
            _chainOverlay?.Bind(_pawn.Inventory);

            Refresh();
        }

        private void RebuildSlotsIfNeeded(Vector2Int gridSize)
        {
            var required = gridSize.x * gridSize.y;
            if (_slots != null && _slots.Length == required && _builtForSize == gridSize)
                return;

            if (_slots != null)
                foreach (var slot in _slots)
                    if (slot != null) Destroy(slot.gameObject);

            _grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            _grid.constraintCount = gridSize.x;

            _slots = new SlotView[required];
            for (var i = 0; i < required; i++)
            {
                var gridPos = new Vector2Int(i % gridSize.x, i / gridSize.x);
                var slot    = Instantiate(_slotPrefab, _grid.transform);
                slot.Initialize(_grid, gridPos, _dragController);
                _slots[i] = slot;
            }

            _builtForSize = gridSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_grid.transform);
        }

        private void OnContentsChanged(
            IReadOnlyDictionary<UnityEngine.Vector2Int, Code.Runtime.Container.Items.ITetrisItem> _)
            => Refresh();

        private void Refresh()
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                var pos = new UnityEngine.Vector2Int(
                    i % _pawn.Inventory.GridSize.x,
                    i / _pawn.Inventory.GridSize.x);

                _slots[i].RefreshView(
                    _pawn.Inventory.Contents.TryGetValue(pos, out var item) ? item : null);
            }
        }
    }

    public interface IInventoryView
    {
        IReadOnlyList<ISlotView> Slots { get; }
        void RefreshView(IPawn pawn);
    }
}