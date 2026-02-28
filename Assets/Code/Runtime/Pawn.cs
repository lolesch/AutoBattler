using System.Collections.Generic;
using Code.Data.Pawns;
using Code.Runtime.Container;
using Code.Runtime.Container.Items;
using Code.Runtime.Grids;
using Code.Runtime.Statistics;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Runtime
{
    public sealed class Pawn : MonoBehaviour, IDamageable
    {
        [SerializeField] private PawnConfig config;
        [SerializeField] public PawnStats stats;
        
        [SerializeField, ReadOnly, PreviewIcon] private Sprite icon;
        
        [SerializeField] public TetrisContainer inventory;
        [SerializeField] public PawnEffect pawnEffects;
        
        private void OnValidate() => SpawnPawn();
        private void Awake() => SpawnPawn();

        [ContextMenu("Spawn")]
        private void SpawnPawn()
        {
            if( !config )
            {
                Debug.LogError("Missing Config to draw from");
                return;
            }
            
            icon = config.icon;
            stats = new PawnStats( config );
            inventory = new TetrisContainer( new Vector2Int( 6, 2 ) );
            
            stats.health.OnDepleted += DespawnPawn;
        }

        private void DespawnPawn()
        {
            Debug.Log( $"{gameObject.name} has been defeated!" );
            gameObject.SetActive(false);
        }
        
        // TODO: transform into command pattern
        public void TakeDamage( float damage ) => stats.health.ReduceCurrent( damage );

        public void EquipItem( TetrisItem item )
        {
            if( !inventory.TryAdd( item ) ) 
                return;
            
            foreach( var affix in item.Affixes )
                stats.ApplyMod( affix );
        }
    }

    public interface IDamageable
    {
        void TakeDamage( float damage );
    }
}
