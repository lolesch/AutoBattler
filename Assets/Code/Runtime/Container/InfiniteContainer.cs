using System;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Container.Items;
using UnityEngine;

namespace Code.Runtime.Container
{
    [Serializable]
    public sealed class InfiniteContainer : IInfiniteContainer
    {
        [SerializeField] private List<ItemStack> contents = new();

        public List<ItemStack> Contents => contents;
        public event Action<List<ItemStack>> OnContentsChanged;

        public void Add( ItemStack arrival )
        {
            if( !arrival.hasValidItem )
                return;

            if( !Merge( ref arrival ) )
                Contents.Add( arrival );

            OnContentsChanged?.Invoke( Contents );
        }

        public bool TryRemove( ItemStack itemStack )
        {
            if( !Contents.Remove( itemStack ) )
                return false;

            OnContentsChanged?.Invoke( Contents );
            return true;
        }

        private bool Merge( ref ItemStack arrival )
        {
            for( var i = 0; i < Contents.Count && 0 < arrival.Amount; i++ )
            {
                if( Contents[i].Item.Equals( arrival.Item ) )
                {
                    var added = Contents[i].Add( arrival.Amount );
                    _ = arrival.Remove( added );
                }
            }

            return arrival.Amount == 0;
        }
    }
}