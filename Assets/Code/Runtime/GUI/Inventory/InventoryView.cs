using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.GUI.Inventory
{
    public sealed class InventoryView : MonoBehaviour
    {
        [SerializeField] private List<SlotView> slots;

        public void RefreshView( Pawn pawn)
        {
            for (var i = 0; i < slots.Count; i++)
                slots[i].RefreshView( pawn.inventory[i] );
        }
    }
}