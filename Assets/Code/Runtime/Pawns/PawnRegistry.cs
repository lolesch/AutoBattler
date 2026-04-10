using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;

namespace Code.Runtime.Pawns
{
    public sealed class PawnRegistry
    {
        private readonly List<IPawn> _playerPawns = new();
        private readonly List<IPawn> _enemyPawns = new();

        public IReadOnlyList<IPawn> playerPawns => _playerPawns;
        public IReadOnlyList<IPawn> enemyPawns => _enemyPawns;
        public IReadOnlyList<IPawn> allPawns => _playerPawns.Concat(_enemyPawns).ToList();

        public event Action<IPawn> OnPawnRegistered;
        public event Action<IPawn> OnPawnUnregistered;

        public void Register(IPawn pawn)
        {
            var list = pawn.Team == PawnTeam.Player ? _playerPawns : _enemyPawns;
            if (list.Contains(pawn)) return;
            list.Add(pawn);
            
            OnPawnRegistered?.Invoke(pawn);
        }

        public void Unregister(IPawn pawn)
        {
            var list = pawn.Team == PawnTeam.Player ? _playerPawns : _enemyPawns;
            if (list.Remove(pawn))
            {
                OnPawnUnregistered?.Invoke(pawn);
            }
        }

        public void ClearEnemies()
        {
            // Enemies are usually destroyed on map change
            foreach (var enemy in _enemyPawns) 
                Unregister(enemy);
            _enemyPawns.Clear();
        }
    }
}