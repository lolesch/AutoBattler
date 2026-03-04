using System.Collections.Generic;
using Code.Runtime.Container;
using Code.Runtime.Container.Items.Chain;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime
{
    /// <summary>
    /// Owns all combat state for a single pawn.
    /// Subscribes to inventory changes and rebuilds one Timer per weapon chain.
    /// Receives its target externally — target finding is the coordinator's responsibility.
    /// </summary>
    public sealed class PawnCombatController : IPawnCombatController
    {
        private readonly ITetrisContainer _inventory;
        private readonly List<Timer>      _weaponTimers = new();

        private IDamageable _target;
        private bool        _isRunning;

        public PawnCombatController(ITetrisContainer inventory)
        {
            _inventory = inventory;
            _inventory.OnContentsChanged += _ => RebuildTimers();
        }

        public void SetTarget(IDamageable target) => _target = target;

        public void StartCombat()
        {
            _isRunning = true;
            RebuildTimers();
        }

        public void StopCombat()
        {
            _isRunning = false;
            StopAllTimers();
        }

        // ── Internal ──────────────────────────────────────────────────────

        private void RebuildTimers()
        {
            StopAllTimers();

            if (!_isRunning)
                return;

            var chains = ChainResolver.Resolve(_inventory);

            if (chains.Count == 0)
            {
                Debug.Log("[Combat] No weapons — pawn is not attacking.");
                return;
            }

            foreach (var chain in chains)
            {
                var resolved = chain.Resolve();

                Debug.Log($"[Combat:{chain.Weapon.Name}] " +
                          $"Damage:{resolved.Damage} | " +
                          $"Speed:{resolved.AttackSpeed} | " +
                          $"Cost:{resolved.ResourceCost}");

                var capturedDamage = resolved.Damage;
                var timer          = new Timer(resolved.AttackSpeed, true);
                timer.OnRewind += () => _target?.TakeDamage(capturedDamage);
                timer.Start();
                _weaponTimers.Add(timer);
            }
        }

        private void StopAllTimers()
        {
            foreach (var timer in _weaponTimers)
                timer.Stop();
            _weaponTimers.Clear();
        }
    }

    public interface IPawnCombatController
    {
        void SetTarget(IDamageable target);
        void StartCombat();
        void StopCombat();
    }
}