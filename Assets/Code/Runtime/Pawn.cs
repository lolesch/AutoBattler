using System.Collections.Generic;
using Code.Data.SO;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Runtime
{
    public sealed class Pawn : MonoBehaviour
    {
        // TODO: make constructor, object pool the prefab and populate scene on the fly
        [SerializeField] private PawnConfig config;
        [SerializeField] public PawnStats stats;
        
        [SerializeField, ReadOnly, PreviewIcon] private Sprite icon;
        
        [SerializeField] private List<Item> items;
        
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
        public void ReceiveDamage( float damage ) => stats.health.ReduceCurrent( damage );

        public void EquipItem( Item item )
        {
            items.Add( item );
            foreach( var affix in item.Affixes )
                stats.ApplyMod( affix );
        }
    }
}
