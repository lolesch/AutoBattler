using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.Grids.RectGridInspector
{
    // based on: https://github.com/Eldoir/Array2DEditor
    [Serializable]
    public abstract class RectGrid<T>
    {
        public const int defaultGridSize = 1;

        [SerializeField] protected Vector2Int gridSize = Vector2Int.one * defaultGridSize;

#pragma warning disable 414
        /// <summary>
        /// NOTE: Only used to display the cells in the Editor. This won't affect the build.
        /// </summary>
        [SerializeField]
        private Vector2Int cellSize;
#pragma warning restore 414

        protected abstract RectRow<T> GetCellRow(int i);

        public T[,] GetCells()
        {
            var cells = new T[gridSize.y, gridSize.x];

            for (var y = 0; y < gridSize.y; y++)
                for (var x = 0; x < gridSize.x; x++)
                    cells[y, x] = GetCell(x, y);
            
            return cells;
        }

        public T GetCell(int x, int y) => GetCellRow(y)[x];

        public void SetCell(int x, int y, T value) => GetCellRow(y)[x] = value;
        
        public List<Vector2Int> GetVec2Ints()
        {
            var cells = GetCells();

            var map = new List<Vector2Int>();

            for (int x = 0; x < cells.GetLength(0); x++)
                for (int y = 0; y < cells.GetLength(1); y++)
                    if( IsValid( GetCell( y, x ) ) )
                        map.Add( new( y, x ) );
                
            return map;
        }

        protected abstract bool IsValid(T target);
    }
    
    [Serializable]
    public class RectGridBool : RectGrid<bool>
    {
        public RectGridBool(int gridSize = defaultGridSize)
        {
            this.gridSize = Vector2Int.one * gridSize;
            rows = new RectRowBool[gridSize];
        }
        
        [SerializeField]
        RectRowBool[] rows = new RectRowBool[defaultGridSize];

        protected override RectRow<bool> GetCellRow(int idx) => rows[idx];
        protected override bool IsValid(bool target) => target == true;
    }
}
