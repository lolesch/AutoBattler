using System;
using System.Collections.Generic;
using Code.Data.Items;
using UnityEngine;

namespace Code.Runtime.Inventory
{
    [Serializable]
    public abstract class AbstractItem : IItem
    {
        public Guid Guid { get; }
        [field: SerializeField] public Sprite Icon { get; private set; }

        protected AbstractItem( IItemData itemData )
        {
            Guid = Guid.NewGuid();
            Icon = itemData.Icon;
        }
    
        public abstract List<Vector2Int> GetPointers( Vector2Int position );
    }
    
    public interface IItem
    {
        Guid Guid { get; }
        Sprite Icon { get; }
    }
}