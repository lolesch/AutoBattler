using System.Collections.Generic;
using Code.Data.Pawns;
using Code.Runtime.Container.Items;
using Code.Runtime.HexGrid;
using Code.Runtime.Statistics;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Runtime
{
    public sealed class Pawn : MonoBehaviour, IDamageable
    {
        // TODO: make constructor, object pool the prefab and populate scene on the fly
        [SerializeField] private PawnConfig config;
        [SerializeField] public PawnStats stats;
        
        [SerializeField, ReadOnly, PreviewIcon] private Sprite icon;
        
        [SerializeField] public List<TetrisItem> inventory;
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
            inventory = new List<TetrisItem>();
            
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
            inventory.Add( item );
            foreach( var affix in item.Affixes )
                stats.ApplyMod( affix );
        }
    }

    public interface IDamageable
    {
        void TakeDamage( float damage );
    }
}
