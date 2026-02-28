using System;
using System.Collections.Generic;
using Code.Data.Enums;
using Code.Data.Items;
using Code.Runtime.Grids.RectGridInspector;
using Code.Runtime.Statistics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Runtime.Container.Items
{
    [Serializable]
    public sealed class TetrisItem : AbstractItem
    {
        public readonly RectGridBool Shape;
        public RotationType rotation;
        
        public TetrisItem( ItemConfig itemData ) : base( itemData )
        {
            Shape = itemData.Shape;
            RarityType = GetRandomRarity();
            Affixes.Add( new PawnStatModifier( itemData.statType, new Modifier( itemData.value, itemData.modifierType, guid ) ) );
        }

        public override List<Vector2Int> GetPointers( Vector2Int position) 
        {
            var parts = Shape.GetVec2Ints();
            var anchor = parts[0]; // or however we define the anchor

            var pointers = new List<Vector2Int>();

            // TODO: consider rotation
            foreach ( var part in parts ) 
                pointers.Add( position + part - anchor );

            return pointers;
        }

        public override void Use() => throw new NotImplementedException();

        [field: SerializeField] public RarityType RarityType { get; private set; }
        [field: SerializeField] public List<PawnStatModifier> Affixes { get; private set; } = new();

        private RarityType GetRandomRarity() => (RarityType) Random.Range(0, Enum.GetValues( typeof(RarityType) ).Length);
    }
    
    public enum RotationType
    {
        Deg0 = 0,       // up       north
        Deg90 = 90,     // left     west
        Deg180 = 180,   // down     south
        Deg270 = 270    // right    east
    }
}