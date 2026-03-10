using System.Collections.Generic;
using Code.Data;
using Code.Runtime.Inventory;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Runtime.UI.Inventory
{
    public sealed class SlotView : MonoBehaviour, ISlotView,
        IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image      _icon;
        [SerializeField] private Image      _highlight;
        [SerializeField] private GameObject _pipPrefab;
        [SerializeField] private Sprite     _dashSprite;
        [SerializeField] private Sprite     _arrowSprite;

        [SerializeField, ReadOnly] private Vector2Int _gridPosition;

        private IInventoryDragController _dragController;

        // Keyed by (connectorSlotPos, connectorDirection) — direction is required because
        // a 1x1 item can have multiple connectors at the same cell pointing different ways.
        private readonly Dictionary<(Vector2Int, Vector2Int), Image> _pips = new();

        public RectTransform RectTransform => (RectTransform)transform;
        public Vector2Int    GridPosition  => _gridPosition;

        public void Initialize(Vector2Int gridPosition, IInventoryDragController dragController)
        {
            _gridPosition   = gridPosition;
            _dragController = dragController;
        }

        public void SetHighlight(SlotHighlight highlight)
        {
            if (_icon.color == Color.clear) return;

            _icon.color = highlight == SlotHighlight.Swap
                ? new Color(1.00f, 0.80f, 0.00f, 1f)
                : Color.white;
        }

        public void SetPipState(Vector2Int connectorSlotPos, Vector2Int connectorDirection, PipState state)
        {
            if (!_pips.TryGetValue((connectorSlotPos, connectorDirection), out var pip)) return;
            pip.sprite = state == PipState.Arrow ? _arrowSprite : _dashSprite;
        }

        // ── UGUI event handlers ───────────────────────────────────────────

        public void OnPointerClick(PointerEventData eventData)
            => _dragController?.OnSlotPointerClick(this, eventData.position);

        public void OnBeginDrag(PointerEventData eventData)
            => _dragController?.OnSlotBeginDrag(this, eventData.position);

        // Required by Unity for OnBeginDrag to fire.
        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
            => _dragController?.OnSlotEndDrag(eventData.position);

        public void OnDrop(PointerEventData eventData)
            => _dragController?.OnSlotDrop(this);

        public void OnPointerEnter(PointerEventData eventData)
            => _dragController?.SetHovered(this);

        public void OnPointerExit(PointerEventData eventData)
            => _dragController?.SetHovered(null);

        // ── Item display ──────────────────────────────────────────────────

        private void OnValidate()
        {
            if (_icon == null) return;
            _icon.rectTransform.anchorMin        = new Vector2(0, 1);
            _icon.rectTransform.anchorMax        = new Vector2(0, 1);
            _icon.rectTransform.anchoredPosition = Vector2.zero;
        }

        public void RefreshView(ITetrisItem item)
        {
            ClearPips();

            var hasItem = item != null;

            if (hasItem)
            {
                var origin = item.GetShapeOrigin();
                var dim    = item.GetVisualDimensions();

                _icon.sprite                         = item.Icon;
                _icon.rectTransform.localEulerAngles = new Vector3(0f, 0f, (int)item.rotation * 90f);
                _icon.rectTransform.pivot            = CalculatePivot(origin, item.GetDimensions(), item.rotation);
                _icon.rectTransform.sizeDelta        = dim * Const.InventoryCellSize;

                BuildPips(item);
            }
            else
            {
                _icon.rectTransform.sizeDelta        = Vector2.one * Const.InventoryCellSize;
                _icon.rectTransform.anchoredPosition = Vector2.zero;
                _icon.rectTransform.pivot            = Vector2.up;
            }

            _icon.color = hasItem ? Color.white : Color.clear;
        }

        private void BuildPips(ITetrisItem item)
        {
            if (_pipPrefab == null) return;

            foreach (var (slotPos, direction) in item.GetGridConnectors(_gridPosition))
            {
                var pip   = Instantiate(_pipPrefab, transform).GetComponent<Image>();
                var pipRT = pip.rectTransform;

                pipRT.anchorMin = new Vector2(0, 1);
                pipRT.anchorMax = new Vector2(0, 1);
                pipRT.pivot     = new Vector2(0.5f, 0.5f);
                pipRT.sizeDelta = Vector2.one * Const.InventoryCellSize;

                var offset = slotPos - _gridPosition;
                pipRT.anchoredPosition = new Vector2(
                     (offset.x + 0.5f) * Const.InventoryCellSize,
                    -(offset.y + 0.5f) * Const.InventoryCellSize);

                // Atan2(-y, x) converts Y-down grid direction to screen-space euler angle.
                pipRT.localEulerAngles = new Vector3(0f, 0f,
                    Mathf.Atan2(-direction.y, direction.x) * Mathf.Rad2Deg);

                pip.sprite        = _dashSprite;
                pip.raycastTarget = false;

                _pips[(slotPos, direction)] = pip;
            }
        }

        private void ClearPips()
        {
            foreach (var pip in _pips.Values)
                if (pip != null) Destroy(pip.gameObject);
            _pips.Clear();
        }

        private static Vector2 CalculatePivot(Vector2Int origin, Vector2Int dims, RotationType rotation) =>
            rotation switch
            {
                RotationType.None   => new Vector2(origin.x / (float)dims.x,       1f - origin.y / (float)dims.y),
                RotationType.CCW90  => new Vector2(1f - origin.y / (float)dims.y,  1f - origin.x / (float)dims.x),
                RotationType.CCW180 => new Vector2(1f - origin.x / (float)dims.x,  origin.y / (float)dims.y),
                RotationType.CCW270 => new Vector2(origin.y / (float)dims.y,       origin.x / (float)dims.x),
                _                  => new Vector2(0.5f, 0.5f),
            };
    }

    public interface ISlotView
    {
        RectTransform RectTransform { get; }
        Vector2Int    GridPosition  { get; }
        void Initialize(Vector2Int gridPosition, IInventoryDragController dragController);
        void RefreshView(ITetrisItem item);
        void SetHighlight(SlotHighlight highlight);
        void SetPipState(Vector2Int connectorSlotPos, Vector2Int connectorDirection, PipState state);
    }

    public enum SlotHighlight { None, Swap }
    public enum PipState      { Dash, Arrow }
}