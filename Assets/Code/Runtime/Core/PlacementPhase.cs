using System;
using System.Collections.Generic;
using Code.Runtime.Pawns;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.Core
{
    /// <summary>
    /// Active during pawn placement and item assignment.
    /// Enables Draggable on all player pawns.
    /// Player confirms when satisfied with positions and inventory state.
    /// </summary>
    public sealed class PlacementPhase : IGamePhase
    {
        private readonly Button  _confirmButton;
        private readonly Action  _onConfirm;
        private readonly List<IPawn> _playerPawns;

        public PlacementPhase( List<IPawn> playerPawns, Button confirmButton, Action onConfirm)
        {
            _playerPawns = playerPawns;
            _confirmButton = confirmButton;
            _onConfirm     = onConfirm;
        }

        public void Enter()
        {
            SetPawnDragging(true);

            _confirmButton.gameObject.SetActive(true);
            _confirmButton.onClick.AddListener(OnConfirm);

            Debug.Log("[Phase] Placement — drag units and assign items, then confirm.");
        }

        public void Exit()
        {
            SetPawnDragging(false);

            _confirmButton.onClick.RemoveListener(OnConfirm);
            _confirmButton.gameObject.SetActive(false);
        }

        private void SetPawnDragging(bool enabled)
        {
            foreach (var pawn in _playerPawns)
            {
                //var draggable = pawn.GetComponent<Draggable>();
                //if (draggable != null)
                //    draggable.enabled = enabled;
            }
        }

        private void OnConfirm() => _onConfirm();
    }
}