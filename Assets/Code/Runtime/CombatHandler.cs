using System;
using System.Linq;
using Code.Data.Items;
using Code.Runtime.Container.Items;
using Code.Runtime.GUI;
using Code.Runtime.GUI.Inventory;
using Code.Runtime.HexGrid;
using Submodules.Utility.Extensions;
using Submodules.Utility.Tools.Timer;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Runtime
{
    public class CombatHandler : MonoBehaviour
    {
        [SerializeField] private Pawn player;
        [SerializeField] private Timer playerTimer;
        //[SerializeField] private PawnResourceView playerHealthView;
        [SerializeField] private Pawn enemy;
        [SerializeField] private Timer enemyTimer;
        //[SerializeField] private PawnResourceView enemyHealthView;
        [SerializeField] private ItemConfig itemConfig;
        [SerializeField] private InventoryView inventoryView;
        
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap levelMap;
        [SerializeField] private Tilemap pawnEffectMap;
        [SerializeField] private TileBase effectTile;
        
        private Plane plane = new Plane( Vector3.back, 0f );
        private Vector3Int selectedCell;
        private Camera cam;
        private Pawn selectedPawn;
        

        private void Awake()
        {
            if (!cam) 
                cam = Camera.main;
        }

        
        private void Start()
        {
            playerTimer = new Timer( player.stats.attackSpeed, true );
            playerTimer.OnRewind += () => enemy.TakeDamage( player.stats.damage );
            
            enemyTimer = new Timer( enemy.stats.attackSpeed, true );
            enemyTimer.OnRewind += () => player.TakeDamage( enemy.stats.damage );
            
            //playerHealthView.SetPawn( player.stats.health );
            //enemyHealthView.SetPawn( enemy.stats.health );
        }

        private void Update()
        {
            if( Input.GetKeyDown( KeyCode.Q) )
                player.pawnEffects.Rotate( false );
            if( Input.GetKeyDown( KeyCode.E) )
                player.pawnEffects.Rotate( true );
            
            var ray = cam.ScreenPointToRay( Input.mousePosition );
            if( !plane.Raycast( ray, out var distance ) ) 
                return;
            
            var cell = grid.WorldToCell( ray.GetPoint( distance ) );

            if( !levelMap.HasTile( cell ) || selectedCell == cell ) 
                return;
                
            selectedCell = cell;
            //Debug.Log( selectedCell );
            CheckHexForUnit( );
        }

        [ContextMenu( "StartCombat" )]
        private void StartCombat()
        {
            playerTimer.Start();
            enemyTimer.Start();
        }

        [ContextMenu("AddItemToEnemy")]
        private void AddItemToEnemy() => AddItem(enemy);
        
        [ContextMenu("AddItemToPlayer")]
        private void AddItemToPlayer() => AddItem(player);
        
        private void AddItem( Pawn pawn )
        {
            pawn.EquipItem(new TetrisItem(itemConfig));
            inventoryView.RefreshView(pawn);
        }

        void CheckHexForUnit( )
        {
            pawnEffectMap.ClearAllTiles();

            foreach( var pawn in new[] { player, enemy } )
            {
                var pawnCell = grid.WorldToCell( pawn.transform.position );
                
                if( pawnCell != selectedCell )
                    continue;

                if (selectedPawn != pawn)
                {
                    selectedPawn = pawn;
                    inventoryView.RefreshView( pawn );
                }

                foreach( var hex in pawn.pawnEffects.GetHexes() )
                {
                    var cell = pawnCell.CellToHex().Add( hex ).ToCell();
                    pawnEffectMap.SetTile( cell, effectTile );
                }
            }
        }
    }
}
