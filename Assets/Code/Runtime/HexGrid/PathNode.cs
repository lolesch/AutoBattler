using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.HexGrid
{
    [Serializable]
    public sealed class PathNode : IComparable<PathNode>, IPathNode
    {
        public PathNode(Hex hex, PathNode parent, float priority = 0)
        {
            Hex = hex;
            Parent = parent;
            Priority = priority;
        }

        [field: SerializeField, ReadOnly] public PathNode Parent { get; private set; }
        [field: SerializeField, ReadOnly] public Hex Hex { get; private set; }
        [field: SerializeField, ReadOnly] public float Priority { get; private set; }

        internal List<Hex> ToHexList(bool includeStart = false, bool reverse = false)
        {
            var path = new List<Hex>();
            var current = this;

            while (current.Parent != null)
            {
                path.Add(current.Hex);
                current = current.Parent;
            }

            if (includeStart)
                path.Add(current.Hex);

            if (!reverse)
                path.Reverse();

            return path;
        }

        public int CompareTo(PathNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (other is null) return 1;
            return Priority.CompareTo(other.Priority);
        }
        
        public static PathNode Invalid()
        {
            Debug.LogWarning("No valid path found");
            return null;
        }
    }
    
    public interface IPathNode
    {
        PathNode Parent { get; }
        Hex Hex { get; }
        float Priority { get; }
    }
}