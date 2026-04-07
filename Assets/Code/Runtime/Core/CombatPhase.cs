using System;
using Code.Runtime.Core.Combat;

namespace Code.Runtime.Core
{
    public sealed class CombatPhase : IGamePhase
    {
        private readonly ICombatCoordinator _coordinator;
        private readonly Action             _onVictory;

        public CombatPhase(ICombatCoordinator coordinator, Action onVictory)
        {
            _coordinator = coordinator;
            _onVictory   = onVictory;
        }

        public void Enter()
        {
            _coordinator.StartCombat();
        }

        public void Exit()
        {
            _coordinator.StopCombat();
        }
    }
}