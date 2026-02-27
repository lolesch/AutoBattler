using System;
using UnityEngine;

namespace Code.Runtime.Grids.HexGridInspector
{
    // based on: https://github.com/Eldoir/Array2DEditor
    [Serializable]
    public abstract class HexRow<T>
    {
        [SerializeField]
        private T[] row = new T[HexGrid<T>.defaultRadius * 2 + 1];

        public T this[int i]
        {
            get => row[i];
            set => row[i] = value;
        }
    }

    [Serializable]
    public sealed class HexRowBool : HexRow<bool> {}

}
