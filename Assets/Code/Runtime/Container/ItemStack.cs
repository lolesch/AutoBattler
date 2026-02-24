using System;
using Code.Runtime.Container.Items;
using UnityEngine;

namespace Code.Runtime.Container
{
    [Serializable]
    public class ItemStack : IItemStack
    {
        [field: SerializeField] public AbstractItem Item { get; private set; }
        [field: SerializeField] public int Amount { get;  private set; }

        public ItemStack( AbstractItem item = null, int amount = 0 )
        {
            Item = item;
            Amount = amount;
        }

        public bool hasValidItem => Item != null && 0 < Amount;
        public int spaceLeft => Math.Clamp( (int)Item.stackLimit - Amount, 0, (int)Item.stackLimit );
        public bool hasSpace => 0 < spaceLeft;

        public int Add( int amount )
        {
            if( amount < 0 )
            {
                Debug.LogError( "Cannot add a negative amount." );
                return 0;
            }

            var increase = Math.Min(spaceLeft, amount);
            Amount += increase;

            return increase;
        }

        public int Remove( int amount )
        {
            if( amount < 0 )
            {
                Debug.LogError( "Cannot remove a negative amount." );
                return 0;
            }
            
            var decrease = Math.Min(Amount, amount);
            Amount -= decrease;

            return decrease;
        }

        public bool Equals( ItemStack other ) =>
            Item.Equals( other.Item ) && Amount.Equals( other.Amount );

        public override bool Equals( object obj ) => obj is ItemStack other && Equals( other );
    }
}