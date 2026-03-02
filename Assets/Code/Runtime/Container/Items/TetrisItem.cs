using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly RectGridBool _shape;
        public RotationType rotation;
        
        public TetrisItem( ItemConfig itemData, RotationType rotation ) : base( itemData )
        {
            _shape = itemData.Shape;
            this.rotation = rotation; 
            RarityType = GetRandomRarity();
            Affixes.Add( new PawnStatModifier( itemData.statType, new Modifier( itemData.value, itemData.modifierType, guid ) ) );
        }

        public override List<Vector2Int> GetPointers(Vector2Int position)
        {
            var normalized = GetNormalizedShape();
            return normalized.Select(p => position + p - GetShapeOrigin( normalized )).ToList();
        }
        
        public List<Vector2Int> GetNormalizedShape()
        {
            var parts = _shape.GetVec2Ints();
            var pivot = parts[0];
            var rotated = parts.Select(p => ApplyRotation(p - pivot, rotation)).ToList();

            var minY = rotated.Min(p => p.y);
            var minX = rotated.Min(p => p.x);
            
            var sb = new StringBuilder();
            foreach( var pos in rotated.Select( p => p - new Vector2Int( 0, minY ) ).ToList() )
                sb.Append( pos );
            
            return rotated.Select(p => p - new Vector2Int(minX, minY)).ToList();
        }
        
        public Vector2Int GetShapeOrigin(List<Vector2Int> normalized = null)
        {
            normalized ??= GetNormalizedShape();
            var minXInTopRow = normalized.Where(p => p.y == 0).Min(p => p.x);
            return new Vector2Int(minXInTopRow, 0);
        }
        
        private Vector2Int ApplyRotation(Vector2Int v, RotationType rotation)
        {
            var rotations = (int)rotation; // assumes enum: None=0, CW90=1, CW180=2, CW270=3
            for (var i = 0; i < rotations; i++)
                v = new Vector2Int(v.y, -v.x);
            return v;
        }
        public Vector2Int GetDimensions()
        {
            var normalized = GetNormalizedShape();
            var width = normalized.Max(p => p.x) - normalized.Min(p => p.x) + 1;
            var height = normalized.Max(p => p.y) - normalized.Min(p => p.y) + 1;
            return new Vector2Int(width, height);
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