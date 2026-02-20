using System;
using System.Collections.Generic;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.HexGrid.HexGridInspector.Runtime
{
    // based on: https://github.com/Eldoir/Array2DEditor
    [Serializable]
    public abstract class HexGrid<T>
    {
        public const int defaultRadius = 2;
        public int Diameter => radius * 2 + 1;
        public int Radius => radius;

        [SerializeField] protected int radius = defaultRadius;

#pragma warning disable 414
        /// <summary>
        /// NOTE: Only used to display the cells in the Editor. This won't affect the build.
        /// </summary>
        [SerializeField]
        private Vector2Int cellSize;
#pragma warning restore 414

        protected abstract Row<T> GetRow(int i);

        public T[,] GetCells()
        {
            var cells = new T[Diameter, Diameter];

            for (var y = 0; y < Diameter; y++)
                for (var x = 0; x < Diameter; x++)
                    cells[y, x] = GetCell(x, y);

            return cells;
        }

        public T GetCell(int x, int y) => GetRow(y)[x];

        public void SetCell(int x, int y, T value) => GetRow(y)[x] = value;

        public List<Hex> GetHexes(bool originIsCenter = true)
        {
            var cells = GetCells();

            if (Diameter % 2 == 0 && originIsCenter)
                Debug.LogWarning("This shape has no center");

            var map = new List<Hex>();

            for (int x = 0, r = -Diameter / 2; x < cells.GetLength(0); x++, r++)
                for (int y = 0, q = -Diameter / 2; y < cells.GetLength(1); y++, q++)
                {
                    // skip cells to convert [,] into a hexRange
                    if (x + y < Radius || x + y - Diameter >= Radius)
                        continue;
                    // skip the center
                    /* else if (x == Radius && y == Radius)
                     *  continue;
                     */

                    if (!IsInvalidValue(GetCell(y, x)))
                        map.Add(originIsCenter
                            ? new(q, r)
                            : new(x, y));
                }

            return map;
        }

        protected abstract bool IsInvalidValue(T target);
    }

    [Serializable]
    public class HexGridBool : HexGrid<bool>
    {
        public HexGridBool(int radius = defaultRadius)
        {
            this.radius = radius;
            rows = new RowBool[radius * 2 + 1];
        }

        [SerializeField]
        private RowBool[] rows;

        protected override Row<bool> GetRow(int i) => rows[i];
        protected override bool IsInvalidValue(bool target) => target == false;
    }
}
