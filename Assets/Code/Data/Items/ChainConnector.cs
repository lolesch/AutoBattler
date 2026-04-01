using System;
using UnityEngine;

namespace Code.Data.Items
{
    public enum ConnectorDirection : byte
    {
        Right = 0,
        Up    = 1,
        Left  = 2,
        Down  = 3,
    }

    [Serializable]
    public struct ChainConnector : IChainConnector
    {
        [field: SerializeField] public Vector2Int         position  { get; private set; }
        [field: SerializeField] public ConnectorDirection direction { get; private set; }
    }

    public static class ConnectorDirectionExtensions
    {
        public static Vector2Int ToVector2Int(this ConnectorDirection direction) =>
            direction switch
            {
                ConnectorDirection.Right => new Vector2Int( 1,  0),
                ConnectorDirection.Up    => new Vector2Int( 0, -1), // compensates the item shapes flipped y 
                ConnectorDirection.Left  => new Vector2Int(-1,  0),
                ConnectorDirection.Down  => new Vector2Int( 0,  1), // compensates the item shapes flipped y 
                _                        => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
            };
    }
    
    public interface IChainConnector
    {
        Vector2Int position { get; }
        ConnectorDirection direction { get; }
    }
}