using System;
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
    /// Handles three chain root types: Weapon (timer), Activator (timer or event),
    /// Reactor (event subscription). All cleanup routes through _cleanupActions.
    /// </summary>
    public sealed class PawnCombatController : IPawnCombatController
    {
        private readonly IPawn            _pawn;
        private readonly ITetrisContainer _inventory;
        private readonly List<Action>     _cleanupActions = new();

        private IPawn _target;
        private bool  _isRunning;

        public PawnCombatController(IPawn pawn)
        {
            _pawn      = pawn;
            _inventory = pawn.Inventory;
            _inventory.OnContentsChanged += _ => RebuildChains();
        }

        public void SetTarget(IPawn target) => _target = target;

        public void StartCombat()
        {
            _isRunning = true;
            RebuildChains();
        }

        public void StopCombat()
        {
            _isRunning = false;
            Cleanup();
        }

        // ── Internal ──────────────────────────────────────────────────────

        private void RebuildChains()
        {
            Cleanup();
            if (!_isRunning) return;

            var chains = ChainResolver.Resolve(_inventory);

            if (chains.Count == 0)
            {
                Debug.Log("[Combat] No weapons — pawn is not attacking.");
                return;
            }

            foreach (var chain in chains)
            {
                var resolved = chain.Resolve();
                Debug.Log($"[Combat:{chain.Root.Name}] " +
                          $"Damage:{resolved.Damage} | " +
                          $"Speed:{resolved.AttackSpeed} | " +
                          $"Cost:{resolved.ResourceCost}");

                switch (chain.Root)
                {
                    case IWeaponItem:
                        BuildWeaponTimer(chain, resolved.AttackSpeed, resolved.Damage);
                        break;
                    case IActivatorItem activator:
                        BuildActivator(activator, chain, resolved);
                        break;
                    case IReactorItem reactor:
                        BuildReactor(reactor, chain, resolved.Damage);
                        break;
                }
            }
        }

        private void BuildWeaponTimer(IItemChain chain, float interval, float damage)
        {
            var timer = new Timer(interval, true);
            timer.OnRewind += () => Fire(chain, damage);
            timer.Start();
            _cleanupActions.Add(() => timer.Stop());
        }

        private void BuildActivator(IActivatorItem activator, IItemChain chain, IResolvedWeaponStats resolved)
        {
            switch (activator.ActivatorType)
            {
                case ActivatorType.ModifyCooldown:
                {
                    BuildWeaponTimer(chain, resolved.AttackSpeed * activator.CooldownMultiplier, resolved.Damage);
                    break;
                }
                case ActivatorType.FireWhenManaFull:
                {
                    var damage = resolved.Damage;
                    void OnManaRecharged() => Fire(chain, damage);
                    _pawn.Stats.mana.OnRecharged += OnManaRecharged;
                    _cleanupActions.Add(() => _pawn.Stats.mana.OnRecharged -= OnManaRecharged);
                    break;
                }
            }
        }

        private void BuildReactor(IReactorItem reactor, IItemChain chain, float damage)
        {
            switch (reactor.ReactorType)
            {
                case ReactorType.OnSelfHit:
                {
                    void OnHealthChanged(float prev, float curr, float _)
                    { if (curr < prev) Fire(chain, damage); }
                    _pawn.Stats.health.OnCurrentChanged += OnHealthChanged;
                    _cleanupActions.Add(() => _pawn.Stats.health.OnCurrentChanged -= OnHealthChanged);
                    break;
                }
                case ReactorType.OnManaDeplete:
                {
                    void OnManaDeplete() => Fire(chain, damage);
                    _pawn.Stats.mana.OnDepleted += OnManaDeplete;
                    _cleanupActions.Add(() => _pawn.Stats.mana.OnDepleted -= OnManaDeplete);
                    break;
                }
                case ReactorType.OnEnemyDeath:
                {
                    if (_target == null) break;
                    void OnEnemyDefeated() => Fire(chain, damage);
                    _target.OnDefeated += OnEnemyDefeated;
                    _cleanupActions.Add(() => _target.OnDefeated -= OnEnemyDefeated);
                    break;
                }
            }
        }

        private void Fire(IItemChain chain, float rootDamage)
        {
            if (_target == null) return;
            _target.TakeDamage(rootDamage);
            FirePayloads(chain, rootDamage);
        }

        private void FirePayloads(IItemChain chain, float rootDamage)
        {
            foreach (var item in chain.Modifiers)
            {
                if (item is not IWeaponItem payload) continue;
                if (EvaluatePayloadCondition(payload))
                    _target?.TakeDamage(rootDamage * payload.PayloadDamageMultiplier);
            }
        }

        private bool EvaluatePayloadCondition(IWeaponItem payload) => payload.PayloadCondition switch
        {
            PayloadConditionType.OnHit        => true,
            PayloadConditionType.OnKill       => _target.Stats.health.IsDepleted,
            PayloadConditionType.HealthBelow  => _target.Stats.health.Percentage < payload.PayloadConditionThreshold,
            // TODO: should check owning pawn's resource, not target's — needs pawn resource ref
            PayloadConditionType.ResourceFull => _target.Stats.mana.IsFull,
            _                                 => false,
        };

        private void Cleanup()
        {
            foreach (var action in _cleanupActions) action();
            _cleanupActions.Clear();
        }
    }

    public interface IPawnCombatController
    {
        void SetTarget(IPawn target);
        void StartCombat();
        void StopCombat();
    }
}