using System;
using Code.Runtime.Statistics;
using Submodules.Utility.Tools;
using UnityEngine;

namespace Code.Runtime
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] private Resource health;
        [SerializeField] private Stat damage;
        [SerializeField] private Stat attackSpeed;
        
        private void Awake()
        {
            SpwanPawn();
        }

        private void SpwanPawn()
        {
            // TODO: import start values from database
            health = new Resource( StatType.MaxLife, 100f );
            damage = new Stat( StatType.Damage, 5f );
            attackSpeed = new Stat( StatType.AttackSpeed, .5f );
            
            health.OnDepleted += DespwanPawn;
        }

        private void DespwanPawn()
        {
            Debug.Log( $"{name} has been defeated!" );
            gameObject.SetActive(false);
        }

        // TODO: transform into command pattern
        public Timer StartAttacking( Pawn target )
        {
            var timer = new Timer( attackSpeed );
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
