using System;

namespace Code.Runtime.Container
{
    public interface ISlotContainer
    {
        public ItemStack[] Contents { get; }
        public event Action<ItemStack[]> OnContentsChanged;
        bool TryAdd( ref ItemStack itemStack );
        bool TryAddAt( int slot, ref ItemStack arrival );

        bool TryRemove( int slot, out ItemStack removed );

        bool TryRemove( ItemStack removal );
    }
    
    public interface IGridContainer
    {
        public ItemStack[] Contents { get; }
        public event Action<ItemStack[]> OnContentsChanged;
        bool TryAdd( ref ItemStack itemStack );
        bool TryAddAt( int slot, ref ItemStack arrival );

        bool TryRemove( int slot, out ItemStack removed );

        bool TryRemove( ItemStack remaining );
    }
}