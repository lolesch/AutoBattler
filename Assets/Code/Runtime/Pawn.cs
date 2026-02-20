using System.Collections.Generic;
using Code.Data.Pawns;
using Code.Runtime.HexGrid;
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
        
        [SerializeField] private List<Item> inventory;
        [SerializeField] public List<PawnEffect> pawnEffects;
        
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
            
            stats.health.OnDepleted += DespawnPawn;
        }

        private void DespawnPawn()
        {
            Debug.Log( $"{gameObject.name} has been defeated!" );
            gameObject.SetActive(false);
        }

        
        // TODO: transform into command pattern
        public void TakeDamage( float damage ) => stats.health.ReduceCurrent( damage );

        public void EquipItem( Item item )
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
