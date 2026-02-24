using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.Container.Items
{
    [Serializable]
    public sealed class TetrisItem : AbstractItem
    {
        public readonly Vector2Int[] Shape;
        
        public TetrisItem( IItemData itemData, StackLimitType stackLimit, Vector2Int[] shape ) 
            : base( itemData, stackLimit ) => Shape = shape;

        public override void Use() => throw new NotImplementedException();
        public override void Revert() => throw new NotImplementedException();
        
        // should rotation be stored in the package/container or at item level?
        public override List<Vector2Int> GetPointers( Vector2Int position, RotationType rotation ) 
        {
            var pointers = new List<Vector2Int>();

            // TODO: consider rotation
            foreach ( var part in Shape ) 
                pointers.Add( position + part );

            return pointers;
        }
    }
    
    public enum RotationType
    {
        Deg0 = 0,       // up       north
        Deg90 = 90,     // left     west
        Deg180 = 180,   // down     south
        Deg270 = 270    // right    east
    }
}