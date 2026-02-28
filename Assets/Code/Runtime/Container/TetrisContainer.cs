using System;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Container.Items;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.Runtime.Container
{
    [Serializable]
    public sealed class TetrisContainer : ITetrisContainer
    {
        public TetrisContainer(Vector2Int gridSize) => GridSize = gridSize;

        public Dictionary<Vector2Int, TetrisItem> Contents { get; private set; } = new();
        public readonly Dictionary<Vector2Int, Vector2Int> ContentPointer = new();
        public event Action<Dictionary<Vector2Int, TetrisItem>> OnContentsChanged;
        
        public readonly Vector2Int GridSize;

        public bool TryAdd( TetrisItem arrival )
        {
            if( arrival == null)
                return false;
            
            for( var y = 0; y < GridSize.y; y++ )
                for( var x = 0; x < GridSize.x; x++ )
                {
                    var position = new Vector2Int( x, y );
                    if( IsEmpty( position ) && TryAddAt( position, ref arrival) )
                        return true;
                }
            
            return false;
        }
        public bool TryAddAt( Vector2Int position, ref TetrisItem arrival )
        {
            if ( !CanAddAt( position, arrival, out var other))
                return false;

            if( 0 == other.Count )
                Add( position, arrival );
            else
            {
                if( !TryRemove( other[0], out var removed ) )
                    Debug.LogError( $"failed to remove at {other[0]}" );

                Add( position, arrival );
                arrival = removed;
            }

            return true;
        }
        private void Add( Vector2Int position, TetrisItem arrival )
        {
            var pointers = arrival.GetPointers( position );
            foreach( var pointer in pointers )
            {
                ContentPointer.Add( pointer, position);
                Debug.Log( $"Added pointer {pointer} to {position}" );   
            }

            Contents.Add( position, arrival );
            OnContentsChanged?.Invoke( Contents );
            //arrival.Use();
        }
        
        public bool TryRemove( TetrisItem toRemove ) => Contents.Keys.Any( position => 
            Contents[position] == toRemove && TryRemove( position, out toRemove ) );

        public bool TryRemove( Vector2Int position, out TetrisItem removed )
        {
            if( IsEmpty( position ) )
            {
                removed = null;
                return false;
            }

            removed = Remove( position );
            return true;
        }
        private TetrisItem Remove( Vector2Int position )
        {
            _ = Contents.Remove(position , out var removed );
            
            var pointers = removed.GetPointers( position );
            foreach (var pointer in pointers)
                _ =ContentPointer.Remove( pointer );

            OnContentsChanged?.Invoke(Contents);
            return removed;
        }
        
        private bool CanAddAt( Vector2Int position, TetrisItem item, out List<Vector2Int> other)
        {
            var pointers = item.GetPointers( position );
            if( pointers.Any( pointer => !IsValidPointer( pointer) ) )
            {
                other = null;
                return false;
            }
                
            other = new List<Vector2Int>();

            foreach( var pointer in pointers )
                if( IsOccupied( pointer, out var contentKey ) )
                    other.Add( contentKey );
            
            other = other.Distinct().ToList();
            return other.Count <= 1;
        }

        private bool IsEmpty( Vector2Int pointer ) => IsValidPointer( pointer ) && !ContentPointer.ContainsKey( pointer );
        private bool IsOccupied( Vector2Int pointer, out Vector2Int contentKey ) => ContentPointer.TryGetValue( pointer, out contentKey );
        private bool IsValidPointer( Vector2Int pointer ) => 0 <= pointer.x && pointer.x < GridSize.x && 
                                                      0 <= pointer.y && pointer.y < GridSize.y;
    }

    public interface ITetrisContainer
    {
        public Dictionary<Vector2Int, TetrisItem> Contents { get; }
        public event Action<Dictionary<Vector2Int, TetrisItem>> OnContentsChanged;
        bool TryAdd( TetrisItem item );
        bool TryAddAt( Vector2Int position, ref TetrisItem arrival );
        bool TryRemove( Vector2Int position, out TetrisItem removed );
        bool TryRemove( TetrisItem toRemove );
    }
}