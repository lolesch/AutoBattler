using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.GUI.Inventory
{
    public sealed class InventoryView : MonoBehaviour
    {
        [SerializeField] private List<SlotView> slots;

        public void RefreshView( Pawn pawn)
        {
            for( var i = 0; i < slots.Count; i++ )
            {
                var pos = ToPosition( i, pawn );
                if( pawn.inventory.ContentPointer.TryGetValue( pos, out var key ) )
                {
                    pawn.inventory.Contents.TryGetValue( key, out var item );
                    slots[i].RefreshView( item );
                }
                else 
                    slots[i].RefreshView( null );
            }
        }
        
        private Vector2Int ToPosition( int slot, Pawn pawn ) => new(slot % pawn.inventory.GridSize.x, slot / pawn.inventory.GridSize.x);
        //private int ToSlot(Vector2Int position) => position.x + position.y * pawn.inventory.GridSize.x;
    }
}