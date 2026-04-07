using System;
using System.Collections.Generic;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Runtime.GameLoop
{
    /// <summary>
    /// Starts combat on all pawns when entered.
    /// Tracks remaining enemies via IPawn.OnDefeated.
    /// Transitions to Loot when all enemies are defeated.
    /// </summary>
    public sealed class CombatPhase : IGamePhase
    {
        private readonly List<ICombatParticipant> _playerPawns;
        private readonly List<ICombatParticipant> _enemyPawns;
        private readonly Action _onVictory;

        private int _remainingEnemies;

        public CombatPhase(List<ICombatParticipant> playerPawns, List<ICombatParticipant> enemyPawns, Action onVictory)
        {
            _playerPawns = playerPawns;
            _enemyPawns  = enemyPawns;
            _onVictory   = onVictory;
        }

        public void Enter()
        {
            _remainingEnemies = _enemyPawns.Count;

            foreach (var enemy in _enemyPawns)
                enemy.OnDefeated += OnEnemyDefeated;

            //foreach (var pawn in _playerPawns)
            //    pawn.CombatController.StartCombat();

            //foreach (var pawn in _enemyPawns)
            //    pawn.CombatController.StartCombat();

            Debug.Log("[Phase] Combat started.");
        }

        public void Exit()
        {
            //foreach (var pawn in _playerPawns)
            //    pawn.CombatController.StopCombat();

            //foreach (var pawn in _enemyPawns)
            //    pawn.CombatController.StopCombat();

            //foreach (var enemy in _enemyPawns)
            //    enemy.OnDefeated -= OnEnemyDefeated;
        }

        private void OnEnemyDefeated()
        {
            _remainingEnemies--;
            Debug.Log($"[Phase] Enemy defeated. Remaining: {_remainingEnemies}");

            if (_remainingEnemies <= 0)
                _onVictory();
        }
    }
    
    public interface ICombatParticipant
    {
        event Action OnDefeated;
        PawnTeam Team { get; }
        void RegisterWith(GamePhaseController controller);
        void UnregisterFrom(GamePhaseController controller);
    }
}