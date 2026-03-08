using System;
using UnityEngine;

namespace Code.Data.Items
{
    /// <summary>
    /// Defines a single connection point on an item in local shape space.
    /// LocalPosition: which cell of the item's shape hosts this connector.
    /// Direction: which adjacent cell this connector reaches into, in local space.
    /// Both are rotated at runtime to world space when the item is placed.
    ///
    /// Two items connect when:
    ///   A.GridSlotTarget == B.GridSlotPosition
    ///   AND B.GridDirection == -A.GridDirection
    /// </summary>
    [Serializable]
    public struct ChainConnector
    {
        [field: SerializeField] public Vector2Int        LocalPosition { get; private set; }
        [field: SerializeField] public ConnectorDirection Direction     { get; private set; }
    }

    public enum ConnectorDirection
    {
        Right = 0,
        Up    = 1,
        Left  = 2,
        Down  = 3,
    }

    public static class ConnectorDirectionExtensions
    {
        public static Vector2Int ToVector2Int(this ConnectorDirection direction) =>
            direction switch
            {
                ConnectorDirection.Right => new Vector2Int( 1,  0),
                ConnectorDirection.Up    => new Vector2Int( 0, -1),
                ConnectorDirection.Left  => new Vector2Int(-1,  0),
                ConnectorDirection.Down  => new Vector2Int( 0,  1),
                _                        => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
            };
    }

    public enum ConnectorType
    {
        Generic,
        // Future: Fire, Ice, Lightning, etc.
    }
}