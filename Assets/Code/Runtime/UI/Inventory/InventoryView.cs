using System.Collections.Generic;
using System.Linq;
using Code.Data;
using Code.Runtime.Inventory;
using Code.Runtime.Pawns;
using NaughtyAttributes;
using Submodules.Utility.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.UI.Inventory
{
    public sealed class InventoryView : MonoBehaviour, IInventoryView
    {
        [SerializeField] private SlotView                _slotPrefab;
        [SerializeField] private GridLayoutGroup         _grid;
        [SerializeField] private ChainOverlayView        _chainOverlay;
        [SerializeField] private InventoryDragController _dragController;

        [SerializeField, ReadOnly, AllowNesting] private SlotView[] _slots;

        public IReadOnlyList<ISlotView> Slots => _slots.Cast<ISlotView>().ToList();

        private ITetrisContainer _container;
        private Vector2Int       _builtForSize;

        private void Awake()
        {
            _grid.cellSize        = Const.InventoryCellSize.ToVector2();
            _grid.spacing         = Vector2.zero;
            _grid.padding.left    = Const.InventoryPadding;
            _grid.padding.right   = Const.InventoryPadding;
            _grid.padding.top     = Const.InventoryPadding;
            _grid.padding.bottom  = Const.InventoryPadding;
        }
        
        private void OnEnable()
        {
            if (_container != null)
                _dragController?.Register(_container, Slots);
        }

        private void OnDisable()
        {
            if (_container != null)
                _dragController?.Unregister(_container);
        }

        private void OnDestroy()
        {
            if (_container != null)
                _container.OnContentsChanged -= OnContentsChanged;
        }

        /// <summary>
        /// Binds to a pawn's inventory. Subscribes to inventory change events on the pawn.
        /// Shorthand for RefreshView(pawn.Inventory) that keeps pawn-inventory event wiring clean.
        /// </summary>
        public void RefreshView(IPawn pawn) => RefreshView(pawn.Inventory);

        /// <summary>
        /// Binds directly to any ITetrisContainer — pawn inventory or player stash.
        /// The stash has no pawn; chain overlay still renders for experimentation,
        /// but chain resolution has no combat impact outside of a pawn inventory.
        /// </summary>
        public void RefreshView(ITetrisContainer container)
        {
            if (_container != container)
            {
                if (_container != null)
                    _container.OnContentsChanged -= OnContentsChanged;

                _container = container;
                _container.OnContentsChanged += OnContentsChanged;
            }

            RebuildSlotsIfNeeded(_container.GridSize);

            _dragController?.Register(_container, Slots);
            _chainOverlay?.Bind(_container);

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
                slot.Initialize(gridPos, _dragController);
                _slots[i] = slot;
            }

            _builtForSize = gridSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)_grid.transform);
        }

        private void OnContentsChanged(
            IReadOnlyDictionary<Vector2Int, ITetrisItem> _) => Refresh();

        private void Refresh()
        {
            for (var i = 0; i < _slots.Length; i++)
            {
                var pos = new Vector2Int(
                    i % _container.GridSize.x,
                    i / _container.GridSize.x);

                _slots[i].RefreshView(
                    _container.Contents.TryGetValue(pos, out var item) ? item : null);
            }
        }
    }

    public interface IInventoryView
    {
        IReadOnlyList<ISlotView> Slots { get; }
        void RefreshView(IPawn pawn);
        void RefreshView(ITetrisContainer container);
    }
}