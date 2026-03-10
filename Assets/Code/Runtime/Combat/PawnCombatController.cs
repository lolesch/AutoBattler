using System.Collections.Generic;
using Code.Data.Enums;
using Code.Runtime.Inventory;
using Code.Runtime.Pawns;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime.Combat
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

        private IPawn _target;
        private bool _isRunning;

        public PawnCombatController(ITetrisContainer inventory)
        {
            _inventory = inventory;
            _inventory.OnContentsChanged += _ => RebuildTimers();
        }

        public void SetTarget(IPawn target) => _target = target;

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
                var resolved       = chain.Resolve();
                var capturedChain  = chain;
                var capturedDamage = resolved.Damage;

                Debug.Log($"[Combat:{chain.Root.Name}] " +
                          $"Damage:{resolved.Damage} | " +
                          $"Speed:{resolved.AttackSpeed} | " +
                          $"Cost:{resolved.ResourceCost}");

                var timer = new Timer(resolved.AttackSpeed, true);
                timer.OnRewind += () =>
                {
                    if (_target == null) return;
                    _target.TakeDamage(capturedDamage);
                    FirePayloads(capturedChain, capturedDamage);
                };
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

        private void FirePayloads(IItemChain chain, float rootDamage)
        {
            foreach (var item in chain.Modifiers)
            {
                if (item is not IWeaponItem payload)
                    continue;

                if (EvaluatePayloadCondition(payload))
                    _target?.TakeDamage(rootDamage * payload.PayloadDamageMultiplier);
            }
        }

        private bool EvaluatePayloadCondition(IWeaponItem payload) => payload.PayloadCondition switch
        {
            PayloadConditionType.OnHit        => true,
            PayloadConditionType.OnKill       => _target.Stats.health.IsDepleted,
            PayloadConditionType.HealthBelow  => _target.Stats.health.Percentage < payload.PayloadConditionThreshold,
            PayloadConditionType.ResourceFull => _target.Stats.mana.IsFull,
            _                                 => false,
        };
    }

    public interface IPawnCombatController
    {
        void SetTarget(IPawn target);
        void StartCombat();
        void StopCombat();
    }
}