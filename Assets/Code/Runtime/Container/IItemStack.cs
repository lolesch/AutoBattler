using System;
using Code.Runtime.Container.Items;

namespace Code.Runtime.Container
{
    public interface IItemStack: IEquatable<ItemStack>
    {
        // AbstractContainer Sender { get; }
        AbstractItem Item { get; }
        int Amount { get; }
        bool hasValidItem { get; }
        int spaceLeft { get; }
        
        int Add( int amountToAdd );
        int Remove( int amountToRemove );
    }
}