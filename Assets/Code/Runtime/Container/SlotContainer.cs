using System;
using Code.Runtime.Container.Items;
using UnityEngine;

namespace Code.Runtime.Container
{
    [Serializable]
    public abstract class SlotContainer : ISlotContainer
    {
        [field: SerializeField] public ItemStack[] Contents { get; private set; }

        public event Action<ItemStack[]> OnContentsChanged;

        protected SlotContainer( int capacity ) => Contents = new ItemStack[capacity];
        
        public bool TryAdd( ref ItemStack itemStack )
        {
            if( !itemStack.hasValidItem )
                return false;

            return TryMerge( ref itemStack ) || TryAddToEmpty( itemStack );
        }
        
        public bool TryAddAt( int slot, ref ItemStack arrival )
        {
            if( !arrival.hasValidItem || !IsValidSlot( slot ) )
                return false;

            if( TryCombineAt( slot, ref arrival ) )
                return true;

            SwapAt (slot, ref arrival );
            return true;
        }

        public bool TryRemove( int slot, out ItemStack removed )
        {
            if( IsEmpty( slot ) )
            {
                removed = new ItemStack();
                return false;
            }

            removed = Contents[slot];
            Contents[slot] = new ItemStack();
            
            OnContentsChanged?.Invoke( Contents );
            return true;
        }

        public bool TryRemove( ItemStack removal )
        {
            if( !removal.hasValidItem )
                return false;

            // TODO: TrySplitAt() -> see TryCombineAt() as reference
            // and iterate over all slots to remove from multiple stacks if necessary
            
            var slot = Array.FindIndex( Contents, p => p.Item.Equals( removal.Item ) );
            if( slot < 0 )
                return false;

            if( removal.Amount > Contents[slot].Amount )
                return false;
            
            _ = Contents[slot].Remove( removal.Amount );
            OnContentsChanged?.Invoke( Contents );
            return true;
        }

        //public abstract void UseItemAt( int slot );

        private void SwapAt( int slot, ref ItemStack arrival )
        {
            var previous  = Contents[slot];
            Contents[slot] = arrival;
            arrival = previous;
                
            OnContentsChanged?.Invoke( Contents );
        }
        
        private bool TryMerge( ref ItemStack arrival )
        {
            if( arrival.Item.stackLimit <= StackLimitType.Single)
                return false;
            
            for( var slot = 0; slot < Contents.Length; slot++ )
            {
                if( !TryCombineAt( slot, ref arrival ) )
                    continue;

                if( 0 < arrival.Amount ) 
                    continue;
                
                arrival = new ItemStack();
                OnContentsChanged?.Invoke( Contents );
                return true;
            }
            return false;
        }
        
        private bool TryCombineAt( int slot, ref ItemStack arrival )
        {
            if( IsEmpty( slot ) || !Contents[slot].Item.Equals( arrival.Item ) || !Contents[slot].hasSpace )
                return false;
            
            var added = Contents[slot].Add( arrival.Amount );
            _ = arrival.Remove( added );
            
            OnContentsChanged?.Invoke( Contents );
            return true;
        }
        
        private bool TryAddToEmpty( ItemStack arrival )
        {
            for( var slot = 0; slot < Contents.Length; slot++ )
            {
                if( !IsEmpty( slot ) ) 
                    continue;
         
                if( TryAddAt( slot, ref arrival ) )
                    return true;
            }
            return false;
        }
        
        private bool IsEmpty( int slot ) => IsValidSlot( slot) && !Contents[slot].hasValidItem;
        private bool IsValidSlot( int slot ) => 0 <= slot && slot < Contents.Length;}
}