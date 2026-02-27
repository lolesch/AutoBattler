using System;
using UnityEngine;

namespace Code.Runtime.Grids.RectGridInspector
{
    [Serializable]
    public class RectRow<T>
    {
        [SerializeField]
        private T[] row = new T[RectGrid<T>.defaultGridSize];

        public T this[int i]
        {
            get => row[i];
            set => row[i] = value;
        }
    }

    [Serializable]
    public sealed class RectRowBool : RectRow<bool> {}
}
