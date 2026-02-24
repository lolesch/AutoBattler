using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.Container.Items
{
    [Serializable]
    public abstract class AbstractItem : IItem
    {
        [field: SerializeField] public Sprite Icon { get; protected set; } = null;
        [field: SerializeField] private string _name { get; set; } = "Item";
        
        public readonly IItemData ItemData;

        protected AbstractItem( IItemData itemData, StackLimitType stackLimit )
        {
            ItemData = itemData;
            this.stackLimit = stackLimit;
            guid = Guid.NewGuid();
        }

        public Guid guid { get; }
        public StackLimitType stackLimit { get; }

        public abstract void Use();
        public abstract void Revert();
        
        // refactor? used only in GridContainer -> have an AbstractGridItem for this?
        public virtual List<Vector2Int> GetPointers( Vector2Int position, RotationType rotation ) => new() { position };
    }

    [Serializable]
    public abstract class AbstractGridItem : AbstractItem
    {
        protected AbstractGridItem( IItemData itemData, StackLimitType stackLimit ) : base( itemData, stackLimit )
        {
        }
    }
    
    public interface IItemData
    {
        // DATA -> in ScriptableObject
        // name
        // description
        //[field: SerializeField] public Sprite icon { get; }
    }
    
    public interface IItem //: IEquatable<AbstractItem> // : IItemData
    {
        Guid guid { get; }
        StackLimitType stackLimit { get; }
        void Use();
    }
}