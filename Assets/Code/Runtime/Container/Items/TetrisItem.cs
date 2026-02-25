using System;
using System.Collections.Generic;
using Code.Data.Enums;
using Code.Data.Items;
using Code.Runtime.Statistics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Runtime.Container.Items
{
    [Serializable]
    public sealed class TetrisItem : AbstractGridItem
    {
        public readonly Vector2Int[] Shape;
        
        public TetrisItem( ItemConfig itemData ) : base( itemData )
        {
            Shape = itemData.Shape;
            RarityType = GetRandomRarity();
            Affixes.Add( new PawnStatModifier( itemData.statType, new Modifier( itemData.value, itemData.modifierType, guid ) ) );
        }

        // should rotation be stored in the package/container or at item level?
        public override List<Vector2Int> GetPointers( Vector2Int position, RotationType rotation ) 
        {
            var pointers = new List<Vector2Int>();

            // TODO: consider rotation
            foreach ( var part in Shape ) 
                pointers.Add( position + part );

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