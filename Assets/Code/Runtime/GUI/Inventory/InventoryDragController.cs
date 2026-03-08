using System.Collections.Generic;
using Code.Runtime.Container;
using Code.Runtime.Container.Items;
using Submodules.Utility.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.GUI.Inventory
{
    public sealed class InventoryDragController : MonoBehaviour, IInventoryDragController
    {
        [SerializeField] private Image           _ghostImage;
        [SerializeField] private Canvas          _canvas;
        [SerializeField] private GridLayoutGroup _grid;

        private ITetrisContainer         _container;
        private IReadOnlyList<ISlotView> _slots;
        private ISlotView                _hoveredSlot;

        private ITetrisItem _heldItem;
        private Vector2Int  _grabOffset;
        private Vector2Int  _grabOrigin;
        private Vector2Int  _originalAnchor; // displaced item's pre-swap position
        private Vector2Int  _pickupAnchor;   // original pickup position — never changes mid-session
        private Vector2     _grabSubCellOffset;

        // Each entry records where the dragged item was placed and where the displaced item came from.
        // Cancel unwinds in reverse to restore the full pre-drag state across chained swaps.
        private readonly Stack<SwapRecord> _swapStack = new();

        private readonly struct SwapRecord
        {
            public readonly Vector2Int PlacedAnchor;
            public readonly Vector2Int DisplacedFromAnchor;
            public SwapRecord(Vector2Int placed, Vector2Int displacedFrom)
            {
                PlacedAnchor          = placed;
                DisplacedFromAnchor   = displacedFrom;
            }
        }

        // OnDrop (target) and OnEndDrag (source) both fire for the same drag gesture.
        // OnDrop sets this flag so OnEndDrag knows the drop was already handled.
        private bool _dropProcessed;

        private enum GestureMode { Idle, Click, Drag }
        private GestureMode _gesture;

        private readonly Vector3[] _corners = new Vector3[4];

        private void Start()
        {
            if (_ghostImage != null)
                _ghostImage.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_heldItem == null) return;

            UpdateGhostPosition(Input.mousePosition);
            UpdatePreview();

            if (Input.GetKeyDown(KeyCode.Q))      Rotate(clockwise: false);
            if (Input.GetKeyDown(KeyCode.E))      Rotate(clockwise: true);
            if (Input.GetKeyDown(KeyCode.Escape)) Cancel();
        }

        // ── IInventoryDragController ───────────────────────────────────────

        public void Bind(ITetrisContainer container, IReadOnlyList<ISlotView> slots)
        {
            _container = container;
            _slots     = slots;
        }

        public void OnSlotPointerClick(ISlotView slot, Vector2 screenPos)
        {
            switch (_gesture)
            {
                case GestureMode.Idle:
                    if (TryPickUp(slot, screenPos))
                        _gesture = GestureMode.Click;
                    break;

                case GestureMode.Click:
                    DropAt(slot);
                    break;
            }
        }

        public void OnSlotBeginDrag(ISlotView slot, Vector2 screenPos)
        {
            _dropProcessed = false;

            switch (_gesture)
            {
                case GestureMode.Idle:
                    if (TryPickUp(slot, screenPos))
                        _gesture = GestureMode.Drag;
                    break;

                case GestureMode.Click:
                    _gesture = GestureMode.Drag;
                    break;
            }
        }

        public void OnSlotEndDrag(Vector2 screenPos)
        {
            if (_gesture != GestureMode.Drag) return;

            if (_dropProcessed)
            {
                _dropProcessed = false;
                return;
            }

            Cancel();
        }

        public void OnSlotDrop(ISlotView slot)
        {
            if (_gesture != GestureMode.Drag) return;

            _dropProcessed = true;
            DropAt(slot);
        }

        public void SetHovered(ISlotView slot)
        {
            _hoveredSlot = slot;
            UpdatePreview();
        }

        public void Cancel()
        {
            if (_heldItem == null) return;

            // Unwind swap stack in reverse.
            // Each iteration: remove what was placed, restore held item to its displaced-from
            // position, then hold what was previously placed — ready for the next iteration.
            while (_swapStack.Count > 0)
            {
                var record = _swapStack.Pop();

                if (!_container.TryRemove(record.PlacedAnchor, out var previouslyPlaced))
                {
                    Debug.LogError($"[DragController] Failed to undo swap — could not remove from {record.PlacedAnchor}.");
                    EndDrag();
                    return;
                }

                ITetrisItem displaced = _heldItem;
                if (!_container.TryAddAt(record.DisplacedFromAnchor, ref displaced))
                    Debug.LogError($"[DragController] Failed to restore '{_heldItem?.Name}' to {record.DisplacedFromAnchor}.");

                _heldItem = previouslyPlaced;
            }

            ITetrisItem original = _heldItem;
            if (!_container.TryAddAt(_pickupAnchor, ref original))
                Debug.LogError($"[DragController] Failed to restore '{_heldItem?.Name}' to {_pickupAnchor}.");

            EndDrag();
        }

        // ── Pickup ────────────────────────────────────────────────────────

        private bool TryPickUp(ISlotView slot, Vector2 screenPos)
        {
            var clickedCell = slot.GridPosition;

            if (!_container.ContentPointer.TryGetValue(clickedCell, out var anchor)) return false;
            if (!_container.TryRemove(anchor, out var item))                          return false;

            _heldItem       = item;
            _originalAnchor = anchor;
            _pickupAnchor   = anchor;
            _swapStack.Clear();
            _grabOffset     = clickedCell - anchor;
            _grabOrigin     = item.GetShapeOrigin();

            var gridRT      = (RectTransform)_grid.transform;
            gridRT.GetWorldCorners(_corners);
            var scale       = gridRT.lossyScale;
            var step        = _grid.CellStep();
            var cellCenterX = _corners[0].x + (_grid.padding.left + (clickedCell.x + 0.5f) * step.x) * scale.x;
            var cellCenterY = _corners[2].y  - (_grid.padding.top  + (clickedCell.y + 0.5f) * step.y) * scale.y;
            // (cellCenter - cursor) so it can be added directly in UpdateGhostPosition.
            _grabSubCellOffset = new Vector2(cellCenterX - screenPos.x, cellCenterY - screenPos.y);

            RefreshGhostImage();
            _ghostImage.gameObject.SetActive(true);
            return true;
        }

        // ── Drop ──────────────────────────────────────────────────────────

        private void DropAt(ISlotView slot)
        {
            var targetAnchor = slot.GridPosition - _grabOffset;

            _container.CanAddAt(targetAnchor, _heldItem, out var overlapping);
            var swapAnchor = (overlapping is { Count: 1 }) ? overlapping[0] : (Vector2Int?)null;

            ITetrisItem arrival = _heldItem;
            if (!_container.TryAddAt(targetAnchor, ref arrival))
            {
                Cancel();
                return;
            }

            var swapOccurred = !ReferenceEquals(arrival, _heldItem);
            if (swapOccurred)
            {
                _swapStack.Push(new SwapRecord(targetAnchor, swapAnchor ?? targetAnchor));
                _heldItem          = arrival;
                _originalAnchor    = swapAnchor ?? targetAnchor;
                var dims           = _heldItem.GetDimensions();
                _grabOrigin        = _heldItem.GetShapeOrigin();
                _grabOffset        = new Vector2Int(dims.x / 2, dims.y / 2) - _grabOrigin;
                _grabSubCellOffset = Vector2.zero;
                _gesture           = GestureMode.Click;
                RefreshGhostImage();
            }
            else
            {
                EndDrag();
            }
        }

        private void EndDrag()
        {
            _heldItem   = null;
            _gesture    = GestureMode.Idle;
            _swapStack.Clear();
            _ghostImage.gameObject.SetActive(false);
            ClearAllHighlights();
        }

        // ── Rotation ──────────────────────────────────────────────────────

        private void Rotate(bool clockwise)
        {
            // Preserve the canvas-space vector from item center to cursor through the rotation.
            var grabFromCenter = GrabFromCenter();

            var steps = clockwise ? 3 : 1;
            _heldItem.rotation = (RotationType)(((int)_heldItem.rotation + steps) % 4);

            _grabOrigin = _heldItem.GetShapeOrigin();
            var newDims = _heldItem.GetDimensions();
            var step    = _grid.CellStep();

            // Invert GrabFromCenter to solve for the new float grab offset.
            var grabOffsetFloat = new Vector2(
                 grabFromCenter.x / step.x - _grabOrigin.x - 0.5f + newDims.x * 0.5f,
                -grabFromCenter.y / step.y - _grabOrigin.y - 0.5f + newDims.y * 0.5f
            );

            _grabOffset = new Vector2Int(
                Mathf.RoundToInt(grabOffsetFloat.x),
                Mathf.RoundToInt(grabOffsetFloat.y));

            // Remaining fraction _grabOffset (int) cannot express — converted to screen pixels.
            var remainder = grabOffsetFloat - new Vector2(_grabOffset.x, _grabOffset.y);
            _grabSubCellOffset = new Vector2(
                 remainder.x * step.x * _canvas.scaleFactor,
                -remainder.y * step.y * _canvas.scaleFactor); // negate Y: grid down, screen up

            // TODO: ceiled-center correction for irregular shapes (1×2, 2×3).

            RefreshGhostImage();
        }

        // ── Ghost ─────────────────────────────────────────────────────────

        private void RefreshGhostImage()
        {
            if (_heldItem == null || _ghostImage == null) return;

            var visual = _heldItem.GetVisualDimensions();
            var step   = _grid.CellStep();

            var ghostRT              = _ghostImage.rectTransform;
            ghostRT.pivot            = new Vector2(0.5f, 0.5f);
            ghostRT.sizeDelta        = new Vector2(visual.x * step.x - _grid.spacing.x,
                                                   visual.y * step.y - _grid.spacing.y);
            ghostRT.localEulerAngles = new Vector3(0f, 0f, (int)_heldItem.rotation * 90f);

            _ghostImage.sprite = _heldItem.Icon;
            _ghostImage.color  = new Color(1f, 1f, 1f, 0.70f);
        }

        private void UpdateGhostPosition(Vector2 screenPos)
        {
            if (_heldItem == null || _ghostImage == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform, screenPos, null, out var canvasLocal);

            // _grabSubCellOffset is screen pixels — divide by scaleFactor for canvas local space.
            _ghostImage.rectTransform.anchoredPosition =
                canvasLocal - GrabFromCenter() + _grabSubCellOffset / _canvas.scaleFactor;
        }

        // Canvas-space vector from item bounding box center to the grabbed cell center.
        // Shared invariant between UpdateGhostPosition and Rotate.
        private Vector2 GrabFromCenter()
        {
            var dims = _heldItem.GetDimensions();
            var step = _grid.CellStep();
            return new Vector2(
                (_grabOffset.x + _grabOrigin.x + 0.5f - dims.x * 0.5f) * step.x,
                (dims.y * 0.5f - _grabOffset.y - _grabOrigin.y - 0.5f) * step.y
            );
        }

        // ── Preview ───────────────────────────────────────────────────────

        private void UpdatePreview()
        {
            ClearAllHighlights();

            if (_heldItem == null) return;

            if (_hoveredSlot == null)
            {
                _ghostImage.color = new Color(1f, 0.40f, 0.40f, 0.70f);
                return;
            }

            _ghostImage.color = new Color(1f, 1f, 1f, 0.70f);

            var targetAnchor = _hoveredSlot.GridPosition - _grabOffset;
            var canAdd       = _container.CanAddAt(targetAnchor, _heldItem, out var overlapping);

            if (!canAdd)
            {
                HighlightCells(targetAnchor, SlotHighlight.Invalid);
                return;
            }

            HighlightCells(targetAnchor, SlotHighlight.Valid);

            if (overlapping is { Count: 1 } &&
                _container.Contents.TryGetValue(overlapping[0], out var swapItem))
                HighlightCells(overlapping[0], SlotHighlight.Swap, swapItem);
        }

        private void HighlightCells(Vector2Int anchor, SlotHighlight highlight, ITetrisItem item = null)
        {
            item ??= _heldItem;
            foreach (var ptr in item.GetPointers(anchor))
                GetSlotAt(ptr)?.SetHighlight(highlight);
        }

        private void ClearAllHighlights()
        {
            if (_slots == null) return;
            foreach (var slot in _slots)
                slot.SetHighlight(SlotHighlight.None);
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private ISlotView GetSlotAt(Vector2Int gridPos)
        {
            if (_slots == null                     ||
                gridPos.x < 0                      ||
                gridPos.x >= _container.GridSize.x ||
                gridPos.y < 0                      ||
                gridPos.y >= _container.GridSize.y)
                return null;

            var index = gridPos.y * _container.GridSize.x + gridPos.x;
            return index < _slots.Count ? _slots[index] : null;
        }
    }

    public interface IInventoryDragController
    {
        void Bind(ITetrisContainer container, IReadOnlyList<ISlotView> slots);
        void OnSlotPointerClick(ISlotView slot, Vector2 screenPos);
        void OnSlotBeginDrag(ISlotView slot,    Vector2 screenPos);
        void OnSlotEndDrag(Vector2 screenPos);
        void OnSlotDrop(ISlotView slot);
        void SetHovered(ISlotView slot);
        void Cancel();
    }
}