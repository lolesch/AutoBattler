using System;
using System.Collections.Generic;
using Code.Runtime.HexGrid.HexGridInspector.Runtime;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.HexGrid
{
    [Serializable]
    public sealed class PawnEffect
    {
        [SerializeField] private HexGridBool shape;
        [SerializeField] private int rotation;
        [SerializeField] public string effect;
        
        public List<Hex> GetHexes()
        {
            var shapeHexes = shape.GetHexes();
            
            for( var i = 0; i < shapeHexes.Count; i++ )
            {
                for( var r = 0; r < rotation; r++ )
                    shapeHexes[i] = shapeHexes[i].Rotate( false );
            }
            return shapeHexes;
        }
        
        public void Rotate( bool clockwise ) => rotation = (clockwise ? rotation + 5 : rotation + 1 ) % 6;
    }
}