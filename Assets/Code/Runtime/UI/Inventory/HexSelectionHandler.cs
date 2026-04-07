using System.Linq;
using Code.Runtime.GameLoop;
using Code.Runtime.HexGrid;
using Code.Runtime.Pawns;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Runtime.UI.Inventory
{
    /// <summary>
    /// Handles hex grid mouse selection.
    /// On hover: highlights the pawn's effect shape on the tilemap and refreshes the inventory view.
    /// No combat knowledge — purely selection and UI.
    /// </summary>
    public sealed class HexSelectionHandler : MonoBehaviour
    {
        [SerializeField] private InventoryView inventoryView;
        [SerializeField] private Grid          grid;
        [SerializeField] private Tilemap       levelMap;
        [SerializeField] private Tilemap       pawnEffectMap;
        [SerializeField] private TileBase      effectTile;

        private Plane      _plane = new(Vector3.back, 0f);
        private Vector3Int _selectedCell;
        private IPawn      _selectedPawn;
        private Camera     _cam;

        private void Awake() => _cam = Camera.main;

        private void Update()
        {
            if (_selectedPawn != null)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                    _selectedPawn.PawnEffects.Rotate(false);
                if (Input.GetKeyDown(KeyCode.E))
                    _selectedPawn.PawnEffects.Rotate(true);
            }

            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!_plane.Raycast(ray, out var distance))
                return;

            var cell = grid.WorldToCell(ray.GetPoint(distance));
            if (!levelMap.HasTile(cell) || _selectedCell == cell)
                return;

            _selectedCell = cell;
            CheckHexForUnit();
        }

        private void CheckHexForUnit()
        {
            pawnEffectMap.ClearAllTiles();

            foreach (var p in GamePhaseController.allPawns)
            {
                if(p is not IPawn pawn) return;

                var pawnPos = ((IHexOccupant) pawn).HexPosition;
                var pawnCell = pawnPos.ToCell();

                if (pawnCell != _selectedCell)
                    continue;

                if (_selectedPawn != pawn)
                {
                    _selectedPawn = pawn;
                    inventoryView.RefreshView(pawn);
                }

                foreach (var hex in pawn.PawnEffects.GetHexes())
                {
                    var cell = pawnPos.Add(hex).ToCell();
                    pawnEffectMap.SetTile(cell, effectTile);
                }
            }
        }
    }
}