using System;
using UnityEngine;

namespace Code.Data.Items
{
    /// <summary>
    /// Defines a single connection point on an item in local shape space.
    /// LocalPosition: which cell of the item's shape hosts this connector.
    /// Direction: which adjacent cell this connector reaches into, in local space.
    ///            Must be a unit vector: (1,0), (-1,0), (0,1), or (0,-1).
    /// Both are rotated at runtime to world space when the item is placed.
    ///
    /// Two items connect when:
    ///   A.GridSlotTarget == B.GridSlotPosition
    ///   AND B.GridDirection == -A.GridDirection
    /// </summary>
    [Serializable]
    public struct ChainConnector
    {
        [field: SerializeField] public Vector2Int LocalPosition { get; private set; }
        [field: SerializeField] public Vector2Int Direction     { get; private set; }
    }

    public enum ConnectorType
    {
        Generic,
        // Future: Fire, Ice, Lightning, etc.
    }
}