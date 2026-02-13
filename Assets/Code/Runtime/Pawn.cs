using System.Collections.Generic;
using Code.Data.SO;
using Code.Data.Statistics;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime
{
    public sealed class Pawn : MonoBehaviour
    {
        // TODO: make constructor, object pool the prefab and populate scene on the fly
        [SerializeField] private PawnConfig config;
        
        [SerializeField, ReadOnly, PreviewIcon] private Sprite icon;
        [SerializeField, ReadOnly] private Resource health;
        [SerializeField, ReadOnly] private Stat damage;
        [SerializeField, ReadOnly] private Stat attackSpeed;
        
        [SerializeField] public List<Item> items;
        
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
            health = new Resource( StatType.MaxLife, config.baseHealth );
            damage = new Stat( StatType.Damage, config.baseDamage );
            attackSpeed = new Stat( StatType.AttackSpeed, config.baseAttackSpeed );
            
            health.OnDepleted += DespawnPawn;
        }

        private void DespawnPawn()
        {
            Debug.Log( $"{gameObject.name} has been defeated!" );
            gameObject.SetActive(false);
        }

        // TODO: transform into command pattern
        public Timer StartAttacking( Pawn target )
        {
            var timer = new Timer( attackSpeed, true );
            timer.OnRewind += () => target?.ReceiveDamage( damage );
            return timer;
        }
        
        // TODO: transform into command pattern
        public void ReceiveDamage( float damage ) => health.ReduceCurrent( damage );

        public void AddDamage()
        {
            damage.AddModifier( new Modifier( 10, ModifierType.FlatAdd, null ) );
        }
    }
}
