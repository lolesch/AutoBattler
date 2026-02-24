using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.Container.Items
{
    [Serializable]
    public abstract class RectItem : AbstractItem
    {
        public readonly Vector2Int Dimensions;
        protected RectItem( IItemData itemData, StackLimitType stackLimit ) : base( itemData, stackLimit ) {}
        
        public override List<Vector2Int> GetPointers( Vector2Int position, RotationType rotation ) 
        {
            // TODO: consider rotation
            var corner = position + Dimensions;
            var pointers = new List<Vector2Int>();

            for (var x = position.x; x < corner.x; x++) 
            for (var y = position.y; y < corner.y; y++) 
                pointers.Add( new Vector2Int( x, y ) );

            return pointers;
        }
    }
}