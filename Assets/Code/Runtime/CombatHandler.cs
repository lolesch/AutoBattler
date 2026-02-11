using System;
using Submodules.Utility.Tools;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime
{
    public class CombatHandler : MonoBehaviour
    {
        [SerializeField] private Pawn player;
        [SerializeField] private Timer playerTimer;
        [SerializeField] private Pawn enemy;
        [SerializeField] private Timer enemyTimer;

        [ContextMenu( "StartCombat" )]
        private void StartCombat()
        {
            playerTimer.Start();
            enemyTimer.Start();
        }
        
        private void Start()
        {
            playerTimer = player.StartAttacking( enemy );
            enemyTimer = enemy.StartAttacking( player );
            
            player.AddDamage();
        }
    }
}
