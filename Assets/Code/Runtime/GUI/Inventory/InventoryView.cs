using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.GUI.Inventory
{
    public sealed class InventoryView : MonoBehaviour
    {
        [SerializeField] private List<SlotView> slots;
        public IReadOnlyList<SlotView> Slots => slots;

        [SerializeField] private ChainOverlayView _chainOverlay;

        private void Awake() => _chainOverlay ??= GetComponent<ChainOverlayView>();

        public void RefreshView(IPawn pawn)
        {
            for (var i = 0; i < slots.Count; i++)
            {
                var pos = ToPosition(i, pawn);
                slots[i].RefreshView(pawn.Inventory.Contents.TryGetValue(pos, out var item) ? item : null);
            }

            _chainOverlay?.Bind(pawn.Inventory);
        }

        private Vector2Int ToPosition(int slot, IPawn pawn) => new(slot % pawn.Inventory.GridSize.x, slot / pawn.Inventory.GridSize.x);
    }
}
