using System;
using Code.Runtime.Container.Items;

namespace Code.Runtime.Container
{
    [Serializable]
    public sealed class EnumSlotContainer<T> : SlotContainer where T : Enum
    {
        public EnumSlotContainer() : base( Enum.GetValues( typeof( T ) ).Length ) { }
        
        public bool TryAdd( ref ItemStack arrival )
        {
            if( !arrival.hasValidItem || arrival.Item is not ISlotTypeItem<T> item )
                return false;
            return TryAddAt( ToInt( item.SlotType ), ref arrival );
        }
        public bool TryAdd( T slot, ref ItemStack arrival ) => TryAdd( ToInt( slot ), ref arrival );
        public bool TryAdd( int slot, ref ItemStack arrival )
        {
            if( !arrival.hasValidItem || arrival.Item is not ISlotTypeItem<T> item || ToInt( item.SlotType ) != slot )
                return false;
            return TryAddAt( slot, ref arrival );
        }

        public bool TryRemove( T slot, out ItemStack removed ) => TryRemove( ToInt( slot ), out removed );

        private int ToInt( T slot ) => Array.IndexOf( Enum.GetValues( typeof( T ) ), slot );
    }
}
