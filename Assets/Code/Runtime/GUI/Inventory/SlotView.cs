using Code.Runtime.Container.Items;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Runtime.GUI.Inventory
{
    public sealed class SlotView : MonoBehaviour, ISlotView,
        IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _highlight;

        [SerializeField, ReadOnly]       private Vector2Int              _gridPosition;
        [SerializeField, ReadOnly]       private GridLayoutGroup         _grid;

        private IInventoryDragController _dragController;

        private float CellSize => _grid.cellSize.x;
        private float Spacing  => _grid.spacing.x;

        private readonly Vector3[] _corners = new Vector3[4];

        // ── ISlotView ──────────────────────────────────────────────────────

        public RectTransform RectTransform => (RectTransform)transform;
        public Vector2Int    GridPosition  => _gridPosition;

        public void Initialize(GridLayoutGroup grid, Vector2Int gridPosition, IInventoryDragController dragController)
        {
            _grid           = grid;
            _gridPosition   = gridPosition;
            _dragController = dragController;
        }

        public void SetHighlight(SlotHighlight highlight)
        {
            if (_highlight == null) return;

            // TODO: source colors from a HighlightColorsSO instead of hardcoding.
            _highlight.color = highlight switch
            {
                SlotHighlight.Valid   => new Color(0.20f, 1.00f, 0.20f, 0.30f),
                SlotHighlight.Swap    => new Color(1.00f, 0.80f, 0.00f, 0.45f),
                SlotHighlight.Invalid => new Color(1.00f, 0.20f, 0.20f, 0.35f),
                _                    => Color.clear,
            };
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
            var hasItem = item != null;

            if (hasItem)
            {
                var origin = item.GetShapeOrigin();
                var visual = item.GetVisualDimensions();

                _icon.sprite                         = item.Icon;
                _icon.rectTransform.localEulerAngles = new Vector3(0f, 0f, (int)item.rotation * 90f);
                _icon.rectTransform.pivot            = CalculatePivot(origin, item.GetDimensions(), item.rotation);
                _icon.rectTransform.sizeDelta        = new Vector2(
                    visual.x * CellSize + (visual.x - 1) * Spacing,
                    visual.y * CellSize + (visual.y - 1) * Spacing
                );
            }
            else
            {
                _icon.rectTransform.sizeDelta        = Vector2.one * CellSize;
                _icon.rectTransform.anchoredPosition = Vector2.zero;
            }

            _icon.color = hasItem ? Color.white : Color.clear;
        }

        private static Vector2 CalculatePivot(Vector2Int origin, Vector2Int dims, RotationType rotation) =>
            rotation switch
            {
                RotationType.None   => new Vector2(origin.x / (float)dims.x,       1f - origin.y / (float)dims.y),
                RotationType.CCW90  => new Vector2(1f - origin.y / (float)dims.y,  1f - origin.x / (float)dims.x),
                RotationType.CCW180 => new Vector2(1f - origin.x / (float)dims.x,  origin.y / (float)dims.y),
                RotationType.CCW270 => new Vector2(origin.y / (float)dims.y,       origin.x / (float)dims.x),
                _                   => new Vector2(0.5f, 0.5f),
            };
    }

    public interface ISlotView
    {
        RectTransform RectTransform { get; }
        Vector2Int    GridPosition  { get; }
        void Initialize(GridLayoutGroup grid, Vector2Int gridPosition, IInventoryDragController dragController);
        void RefreshView(ITetrisItem item);
        void SetHighlight(SlotHighlight highlight);
    }

    public enum SlotHighlight { None, Valid, Swap, Invalid }
}