using System;
using Code.Data.SO;
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
        [SerializeField] private ItemConfig itemConfig;

        [ContextMenu( "StartCombat" )]
        private void StartCombat()
        {
            playerTimer = player.StartAttacking( enemy );
            enemyTimer = enemy.StartAttacking( player );
            
            playerTimer.Start();
            enemyTimer.Start();
        }

        [ContextMenu( "AddItem" )]
        private void AddItem()
        {
            var item = new Item( itemConfig );
            item.Initialize();
            player.items.Add( item );
        }
    }
}
