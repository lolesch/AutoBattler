using System.Collections.Generic;
using Code.Data.Enums;
using Code.Runtime.Grids.RectGridInspector;
using Submodules.Utility.Attributes;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Data.Items
{
    public abstract class ItemConfig : ScriptableObject, IItemData
    {
        [field: SerializeField, PreviewIcon] public Sprite       Icon  { get; private set; }
        [field: SerializeField]              public RectGridBool Shape { get; private set; }

        public abstract int MaxConnectors { get; }
        
        [SerializeField] private List<ChainConnector> connectors = new();
        public IReadOnlyList<ChainConnector> Connectors => connectors;

        protected virtual void OnValidate()
        {
            connectors ??= new List<ChainConnector>(MaxConnectors);

            while (connectors.Count < MaxConnectors)
                connectors.Add(new ChainConnector());

            while (connectors.Count > MaxConnectors)
                connectors.RemoveAt(connectors.Count - 1);

            var shapeCells = Shape != null ? Shape.GetVec2Ints() : null;
            for (var i = 0; i < connectors.Count; i++)
            {
                var c = connectors[i];
                if (shapeCells != null && !shapeCells.Contains(c.position))
                    Debug.LogWarning($"[{name}] Connector {i}: LocalPosition {c.position} is not part of the item shape.", this);
                if (shapeCells != null && shapeCells.Contains(c.position + c.direction.ToVector2Int()))
                    Debug.LogWarning($"[{name}] Connector {i}: Direction points inward — target cell {c.position + c.direction.ToVector2Int()} is part of the item shape.", this);
                for (var j = i + 1; j < connectors.Count; j++)
                    if (c.position == connectors[j].position && c.direction.ToVector2Int() == connectors[j].direction.ToVector2Int())
                        Debug.LogWarning($"[{name}] Connectors {i} and {j} are identical (position {c.position}, direction {c.direction}) — remove the duplicate.", this);
            }
        }
    }

    public interface IItemData
    {
        Sprite                        Icon       { get; }
        RectGridBool                  Shape      { get; }
        IReadOnlyList<ChainConnector> Connectors { get; }
    }
}