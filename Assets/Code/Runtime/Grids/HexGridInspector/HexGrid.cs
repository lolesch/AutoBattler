using System;
using System.Collections.Generic;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.Grids.HexGridInspector
{
    [Serializable]
    public abstract class HexGrid<T>
    {
        public const int defaultRadius = 2;
        public int Diameter => radius * 2 + 1;
        public int Radius   => radius;

        [SerializeField] protected int radius = defaultRadius;

#pragma warning disable 414
        [SerializeField] private Vector2Int cellSize;
#pragma warning restore 414

        protected abstract HexRow<T> GetRow(int i);

        public T[,] GetCells()
        {
            var cells = new T[Diameter, Diameter];
            for (var y = 0; y < Diameter; y++)
                for (var x = 0; x < Diameter; x++)
                    cells[y, x] = GetCell(x, y);
            return cells;
        }

        public T    GetCell(int x, int y)          => GetRow(y)[x];
        public void SetCell(int x, int y, T value) => GetRow(y)[x] = value;

        public List<Hex> GetHexes()
        {
            var cells = GetCells();
            var map   = new List<Hex>();

            for (int x = 0, r = -Diameter / 2; x < cells.GetLength(0); x++, r++)
                for (int y = 0, q = -Diameter / 2; y < cells.GetLength(1); y++, q++)
                {
                    if (x + y < Radius || x + y - Diameter >= Radius)
                        continue;

                    if (IsValid(GetCell(y, x)))
                        map.Add(new(q, r));
                }

            return map;
        }

        protected abstract bool IsValid(T target);
    }

    [Serializable]
    public class HexGridBool : HexGrid<bool>
    {
        public HexGridBool(int radius = defaultRadius)
        {
            this.radius = radius;
            rows        = new HexRowBool[radius * 2 + 1];
        }

        // Field initializer ensures rows is never null when Unity deserializes
        // a fresh instance without calling the parameterized constructor.
        // Mirrors the pattern used by RectGridBool.
        [SerializeField] private HexRowBool[] rows = new HexRowBool[defaultRadius * 2 + 1];

        protected override HexRow<bool> GetRow(int i) => rows[i];
        protected override bool IsValid(bool target)  => target == true;
    }
}