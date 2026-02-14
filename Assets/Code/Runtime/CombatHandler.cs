using Code.Data.SO;
using Code.Runtime.GUI;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime
{
    public class CombatHandler : MonoBehaviour
    {
        [SerializeField] private Pawn player;
        [SerializeField] private Timer playerTimer;
        [SerializeField] private PawnResourceView playerHealthView;
        [SerializeField] private Pawn enemy;
        [SerializeField] private Timer enemyTimer;
        [SerializeField] private PawnResourceView enemyHealthView;
        
        [SerializeField] private ItemConfig itemConfig;

        private void Start()
        {
            playerTimer = new Timer( player.stats.attackSpeed, true );
            playerTimer.OnRewind += () => enemy.ReceiveDamage( player.stats.damage );
            
            enemyTimer = new Timer( enemy.stats.attackSpeed, true );
            enemyTimer.OnRewind += () => player.ReceiveDamage( enemy.stats.damage );
            
            playerHealthView.SetPawn( player.stats.health );
            enemyHealthView.SetPawn( enemy.stats.health );
        }

        [ContextMenu( "StartCombat" )]
        private void StartCombat()
        {
            playerTimer.Start();
            enemyTimer.Start();
        }

        [ContextMenu( "AddItem" )]
        private void AddItem() => enemy.EquipItem( new Item( itemConfig ) );
    }
}
