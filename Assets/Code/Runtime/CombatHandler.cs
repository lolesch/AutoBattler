using System;
using Submodules.Utility.Tools;
using UnityEngine;

namespace Code.Runtime
{
    public class CombatHandler : MonoBehaviour
    {
        [SerializeField] private Pawn player;
        [SerializeField] private Timer playerTimer;
        [SerializeField] private Pawn enemy;
        [SerializeField] private Timer enemyTimer;

        private void Start() => StartCombat();

        [ContextMenu("StartCombat")]
        void StartCombat()
        {
            playerTimer = player.StartAttacking( enemy );
            enemyTimer = enemy.StartAttacking( player );
            
            player.AddDamage();
        }

        private void FixedUpdate()
        {
            playerTimer?.Tick( Time.fixedDeltaTime );
            enemyTimer?.Tick( Time.fixedDeltaTime );
        }
    }
}
