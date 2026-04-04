using System;
using System.Collections.Generic;
using Code.Runtime.Grids.HexGridInspector;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.Pawns
{
    [Serializable]
    public sealed class PawnEffect : IPawnEffect
    {
        [SerializeField] private HexGridBool shape;
        [SerializeField] private int         rotation;

        [field: SerializeField] public string Effect { get; private set; } // was public field

        public List<Hex> GetHexes()
        {
            var shapeHexes = shape.GetHexes();

            for (var i = 0; i < shapeHexes.Count; i++)
            for (var r = 0; r < rotation; r++)
                shapeHexes[i] = shapeHexes[i].Rotate(false);

            return shapeHexes;
        }

        public void Rotate(bool clockwise) => rotation = (clockwise ? rotation + 5 : rotation + 1) % 6;
    }

    public interface IPawnEffect
    {
        string   Effect   { get; }
        List<Hex> GetHexes();
        void      Rotate(bool clockwise);
    }
}