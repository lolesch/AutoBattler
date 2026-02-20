using System.Linq;
using Code.Data.Items;
using Code.Runtime.GUI;
using Code.Runtime.HexGrid;
using Submodules.Utility.Extensions;
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

        private void OnDestroy() => GridController.OnHexSelected -= CheckHexForUnit;
        private void Awake() => GridController.OnHexSelected += CheckHexForUnit;
        
        private void Start()
        {
            playerTimer = new Timer( player.stats.attackSpeed, true );
            playerTimer.OnRewind += () => enemy.TakeDamage( player.stats.damage );
            
            enemyTimer = new Timer( enemy.stats.attackSpeed, true );
            enemyTimer.OnRewind += () => player.TakeDamage( enemy.stats.damage );
            
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
        
        void CheckHexForUnit( Hex selectedHex )
        {
            //foreach( var pawn in new[] { player, enemy } )
            //{
            //    if( pawn.transform.position.WorldToHex( GridController.HexWidth, GridController.Circumradius ) !=
            //        selectedHex ) continue;

            //    foreach( var hex in pawn.pawnEffects.Select( effect => effect.shape.GetHexes() ).SelectMany( hexes => hexes ) )
            //        GridController.DrawHexagonOnXZPlane( hex, Color.red );
            //}
        }
    }
}
