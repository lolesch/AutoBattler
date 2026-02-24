using System;
using System.Collections.Generic;

namespace Code.Runtime.Container
{
    public interface IInfiniteContainer
    {
        List<ItemStack> Contents { get; }
        event Action<List<ItemStack>> OnContentsChanged;
        void Add( ItemStack arrival );
        bool TryRemove( ItemStack itemStack );
    }
}