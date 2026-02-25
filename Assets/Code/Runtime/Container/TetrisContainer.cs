using System;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Container.Items;
using UnityEngine;

namespace Code.Runtime.Container
{
    [Serializable]
    public sealed class TetrisContainer : ITetrisContainer
    {
        public TetrisContainer(Vector2Int dimensions)
        {
            this.dimensions = dimensions;
            Contents = new TetrisItem[dimensions.x * dimensions.y];
        }
        
        [field: SerializeField] public TetrisItem[] Contents { get; private set; }
        public event Action<TetrisItem[]> OnContentsChanged;
        
        private readonly Vector2Int dimensions;
        private Dictionary<Vector2Int, int> gridPointer = new();

        public bool TryAdd(ref TetrisItem arrival)
        {
            if( arrival == null)
                return false;
            
            for (var slot = 0; slot < Contents.Length; slot++)
                if (IsEmpty(slot) && TryAddAt(slot, ref arrival))
                    return true;
            
            return false;
        }
        public bool TryAddAt(int slot, ref TetrisItem arrival)
        {
            if (!CanAddAt(slot, arrival, out var other))
                return false;

            if( other.Any() )
            {
                if( !TryRemove(other[0], out var removed) )
                    Debug.LogError($"failed to remove at {other[0]}");
                
                Add(slot, arrival);
                arrival = removed;
            }
            else
                Add(slot, arrival);
            
            return true;
        }
        public bool TryRemove(int slot, out TetrisItem removed)
        {
            if (IsEmpty(slot))
            {
                removed = null;
                return false;
            }

            removed = Remove(slot);
            return true;
        }
        public bool TryRemove( TetrisItem toRemove )
        {
            for (var slot = Contents.Length; slot-- > 0;)
                if( Contents[slot] == toRemove && TryRemove(slot, out toRemove))
                    return true;
            
            return false;
        }

        private void Add(int slot, TetrisItem arrival)
        {
            var pointers = arrival.GetPointers(ToPosition(slot), RotationType.Deg0);
            foreach (var pointer in pointers)
                gridPointer.Add(pointer, slot);

            Contents[slot] = arrival;
            OnContentsChanged?.Invoke(Contents);
        }
        private TetrisItem Remove( int slot )
        {
            var removed = Contents[slot];
            
            var pointers = removed.GetPointers(ToPosition(slot), RotationType.Deg0);
            foreach (var pointer in pointers)
                gridPointer.Remove(pointer);

            Contents[slot] = null;
            OnContentsChanged?.Invoke(Contents);
            return removed;
        }

        private int ToSlot(Vector2Int position) => position.x + position.y * dimensions.x;
        private Vector2Int ToPosition(int slot) => new(slot % dimensions.x, slot / dimensions.x);
        
        private List<int> GetOverlappingItems(Vector2Int position, TetrisItem item)
        {
            var slots = new List<int>();

            var pointers = item.GetPointers(position, RotationType.Deg0);

            foreach (var pointer in pointers)
            {
                if (!IsEmpty(pointer, out var otherSlot))
                    slots.Add(otherSlot);
            }

            return slots.Distinct().ToList();
        }
        private bool CanAddAt(int slot, TetrisItem item, out List<int> other)
        {
            var position = ToPosition(slot);
            var pointers = item.GetPointers(position, RotationType.Deg0);

            if (pointers.Any(pointer => !IsValidSlot( ToSlot(position + pointer))))
            {
                other = null;
                return false;
            }
            
            other = GetOverlappingItems(position, item);
            return other.Count <= 1;
        }
        
        private bool IsEmpty(int slot) => IsValidSlot(slot) && Contents[slot] == null;
        private bool IsEmpty(Vector2Int pointer, out int slot) => !gridPointer.TryGetValue(pointer, out slot);
        private bool IsValidSlot(int slot) => 0 <= slot && slot < Contents.Length;
    }

    public interface ITetrisContainer
    {
        public TetrisItem[] Contents { get; }
        public event Action<TetrisItem[]> OnContentsChanged;
        bool TryAdd( ref TetrisItem item );
        bool TryAddAt( int slot, ref TetrisItem arrival );
        bool TryRemove( int slot, out TetrisItem removed );
        bool TryRemove( TetrisItem toRemove );
    }
}