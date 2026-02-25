using System;
using System.Collections.Generic;
using Code.Data.Items;
using UnityEngine;

namespace Code.Runtime.Container.Items
{
    [Serializable]
    public abstract class AbstractItem : IItem
    {
        public Guid guid { get; }
        [field: SerializeField] public Sprite Icon { get; private set; }

        protected AbstractItem( IItemData itemData )
        {
            guid = Guid.NewGuid();
            Icon = itemData.Icon;
        }
        
        public abstract void Use();
        //public abstract void Revert();
    }

    [Serializable]
    public abstract class AbstractGridItem : AbstractItem
    {
        protected AbstractGridItem( IItemData itemData ) : base( itemData ) {}    
        
        public virtual List<Vector2Int> GetPointers( Vector2Int position, RotationType rotation ) => new() { position };
    }
    
    public interface IItem
    {
        Guid guid { get; }
        Sprite Icon { get; }
        void Use();
    }
}