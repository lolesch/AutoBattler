using System;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Container.Items;
using Code.Runtime.Statistics;
using UnityEngine;

namespace Code.Runtime.Container
{
    [Serializable]
    public sealed class TetrisContainer : ITetrisContainer
    {
        public TetrisContainer(Vector2Int gridSize, PawnStats stats)
        {
            GridSize = gridSize;
            _stats = stats;
        }
        public Dictionary<Vector2Int, TetrisItem> Contents { get; private set; } = new();
        public readonly Dictionary<Vector2Int, Vector2Int> ContentPointer = new();
        public event Action<Dictionary<Vector2Int, TetrisItem>> OnContentsChanged;
        
        public readonly Vector2Int GridSize;
        private readonly PawnStats _stats;

        public bool TryAdd( TetrisItem arrival )
        {
            if( arrival == null)
                return false;
            
            for( var x = 0; x < GridSize.x; x++ )
                for( var y = 0; y < GridSize.y; y++ )
                {
                    var position = new Vector2Int( x, y );
                    if( CanAddAt( position, arrival, out var other ) && other.Count == 0 )
                        if( TryAddAt( position, ref arrival ) )
                            return true;
                }
    
            return false;
        }
        public bool TryAddAt( Vector2Int position, ref TetrisItem arrival )
        {
            if ( !CanAddAt( position, arrival, out var other))
                return false;

            if( 0 == other.Count )
                return Add( position, arrival );

            if( !TryRemove( other[0], out var removed ) )
            {
                Debug.LogError( $"failed to remove at {other[0]}" );
                return false;
            }

            if( !Add( position, arrival ) )
            {
                // re-add the removed item since Add failed
                Add( other[0], removed );
                return false;
            }

            arrival = removed;
            return true;
        }
        private bool Add( Vector2Int position, TetrisItem arrival )
        {
            var pointers = arrival.GetPointers(position);
            var addedPointers = new List<Vector2Int>();

            foreach (var pointer in pointers)
            {
                if (!ContentPointer.TryAdd(pointer, position))
                {
                    foreach (var added in addedPointers)
                        ContentPointer.Remove(added);

                    Debug.LogWarning($"Failed to add item at {position}, cell {pointer} already occupied.");
                    return false;
                }
                addedPointers.Add(pointer);
                Debug.Log( $"Added Pointer {pointer}");
            }

            if (!Contents.TryAdd(position, arrival))
            {
                foreach (var added in addedPointers)
                    ContentPointer.Remove(added);

                Debug.LogWarning($"Failed to add item to Contents at {position}.");
                return false;
            }
            Debug.Log( $"Added {arrival} at {position}");
            
            if (arrival is IEquippable equippable)
                equippable.OnEquipped(_stats);
            OnContentsChanged?.Invoke(Contents);
            return true;
        }
        
        public bool TryRemove( TetrisItem toRemove ) => Contents.Keys.Any( position => 
            Contents[position] == toRemove && TryRemove( position, out _ ) );
        public bool TryRemove( Vector2Int position, out TetrisItem removed )
        {
            if( IsEmpty( position ) )
            {
                removed = null;
                return false;
            }

            return Remove( position, out removed );
        }
        private bool Remove( Vector2Int position, out TetrisItem removed )
        {
            if( !Contents.TryGetValue( position, out removed ) )
                return false;

            if( !Contents.Remove( position ) )
                return false;

            var pointers = removed.GetPointers( position );
            foreach( var pointer in pointers )
                ContentPointer.Remove( pointer );

            if( removed is IEquippable equippable )
                equippable.OnUnequipped( _stats );

            OnContentsChanged?.Invoke( Contents );
            return true;
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
