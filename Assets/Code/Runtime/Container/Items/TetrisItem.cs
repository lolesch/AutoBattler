using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using Code.Data.Items;
using Code.Runtime.Grids.RectGridInspector;
using Code.Runtime.Statistics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Runtime.Container.Items
{
    [Serializable]
    public sealed class TetrisItem : AbstractItem, IEquippable
    {
        public readonly RectGridBool Shape;
        public RotationType rotation;
        
        public TetrisItem( ItemConfig itemData, RotationType rotation ) : base( itemData )
        {
            Shape = itemData.Shape;
            this.rotation = rotation; 
            RarityType = GetRandomRarity();
            Affixes.Add( new PawnStatModifier( itemData.statType, new Modifier( itemData.value, itemData.modifierType, guid ) ) );
        }

        public override List<Vector2Int> GetPointers( Vector2Int position) 
        {
            var parts = Shape.GetVec2Ints();
            var pivot = parts[0];
            var rotated = parts.Select(p => ApplyRotation(p - pivot, rotation)).ToList();

            // snap top of shape to y=0
            var minY = rotated.Min(p => p.y);
            var normalized = rotated.Select(p => p - new Vector2Int(0, minY)).ToList();

            // find leftmost occupied cell in top row as placement anchor
            var minXInTopRow = normalized.Where(p => p.y == 0).Min(p => p.x);
            var placementAnchor = new Vector2Int(minXInTopRow, 0);

            return normalized.Select(p => position + p - placementAnchor).ToList();
        }
        
        private Vector2Int ApplyRotation(Vector2Int v, RotationType rotation)
        {
            var rotations = (int)rotation; // assumes enum: None=0, CW90=1, CW180=2, CW270=3
            for (var i = 0; i < rotations; i++)
                v = new Vector2Int(v.y, -v.x);
            return v;
        }
        
        public override void Use() => throw new NotImplementedException();

        [field: SerializeField] public RarityType RarityType { get; private set; }
        [field: SerializeField] public List<PawnStatModifier> Affixes { get; private set; } = new();

        private RarityType GetRandomRarity() => (RarityType) Random.Range(0, Enum.GetValues( typeof(RarityType) ).Length);
        
        void IEquippable.OnEquipped( PawnStats stats )
        {
            foreach( var affix in Affixes )
                stats.ApplyMod( affix );
        }

        void IEquippable.OnUnequipped( PawnStats stats )
        {
            foreach( var affix in Affixes )
                stats.RemoveMod( affix );
        }
    }
    
    public interface IEquippable
    {
        void OnEquipped( PawnStats stats );
        void OnUnequipped( PawnStats stats );
    }
    
    [Serializable]
    public enum RotationType
    {
        None=0, 
        CW90=1, 
        CW180=2, 
        CW270=3
    }
}