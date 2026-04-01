using System;
using System.Collections.Generic;
using Code.Data.Enums;
using Code.Data.Items.Shifter;
using Code.Runtime.Inventory;
using Code.Runtime.Pawns;
using Code.Runtime.Statistics;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime.Combat
{
    /// <summary>
    /// Owns all combat state for a single pawn.
    /// Weapon stats are live MutableFloat objects — amplifier mods are applied once per chain
    /// build, activator mods are added/removed each cycle based on condition state.
    /// All cleanup routes through _cleanupActions.
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
                chain.ApplyChainModifiers();
                _cleanupActions.Add(chain.RemoveChainModifiers);
                //ChainResolver.LogChain(chain);

                switch (chain.Root)
                {
                    case IWeaponItem:
                    case IShifterItem:
                        BuildTimedChain(chain);
                        break;
                    case IReactorItem reactor:
                        BuildReactor(reactor, chain);
                        break;
                }
            }
        }

        /// <summary>
        /// Builds a single timer for the chain. Activator conditions are re-evaluated
        /// on each rewind — modifier is added when condition becomes true, removed when
        /// it falls off. Duration is updated after condition evaluation so the next
        /// cycle reflects the current attack speed.
        /// </summary>
        private void BuildTimedChain(IItemChain chain)
        {
            var weapon     = chain.Weapon;
            var activators = GetActivators(chain);
            var (costResource, genResource) = ResolveChainResources(chain);
            var applied    = new Dictionary<IShifterItem, (Modifier firing, Modifier output)>();

            EvaluateActivators();

            var timer = new Timer(weapon.AttackSpeed, true);
            timer.OnRewind += () =>
            {
                EvaluateActivators();
                timer.Duration = weapon.AttackSpeed;

                if (CanFire(weapon, costResource))
                    Fire(chain, weapon, costResource, genResource);
            };
            timer.Start();

            _cleanupActions.Add(() =>
            {
                timer.Stop();
                foreach (var (act, (firingMod, outputMod)) in applied)
                {
                    weapon.GetUsageStat(act.usageMod.stat).TryRemoveModifier(firingMod);
                    weapon.GetAttackStat(act.attackMod.stat).TryRemoveModifier(outputMod);
                }
                applied.Clear();
            });
            return;

            void EvaluateActivators()
            {
                foreach (var act in activators)
                {
                    if (applied.ContainsKey(act))
                    {
                        var (firingMod, outputMod) = applied[act];
                        weapon.GetUsageStat(act.usageMod.stat).TryRemoveModifier(firingMod);
                        weapon.GetAttackStat(act.attackMod.stat).TryRemoveModifier(outputMod);
                        applied.Remove(act);
                    }
                    else
                    {
                        var firingMod = act.usageMod.modifier;
                        var outputMod = act.attackMod.modifier;
                        weapon.GetUsageStat(act.usageMod.stat).AddModifier(firingMod);
                        weapon.GetAttackStat(act.attackMod.stat).AddModifier(outputMod);
                        applied[act] = (firingMod, outputMod);
                    }
                }
            }
        }

        private void BuildReactor(IReactorItem reactor, IItemChain chain)
        {
            var weapon    = chain.Weapon;
            var condType  = reactor.ConditionType;
            var condThres = reactor.ConditionThreshold;
            var (costResource, genResource) = ResolveChainResources(chain);

            // TODO: merge ConditionMet() and CanFire()
            bool ConditionMet() => EvaluateCondition(condType, condThres);

            switch (reactor.ReactorType)
            {
                case ReactorType.OnSelfHit:
                {
                    void OnHealthChanged(float prev, float curr, float _)
                    { if (curr < prev && ConditionMet() && CanFire(weapon, costResource)) Fire(chain, weapon, costResource, genResource); }
                    _pawn.Stats.health.OnCurrentChanged += OnHealthChanged;
                    _cleanupActions.Add(() => _pawn.Stats.health.OnCurrentChanged -= OnHealthChanged);
                    break;
                }
                case ReactorType.OnManaDeplete:
                {
                    void OnManaDeplete() { if (ConditionMet()&& CanFire(weapon, costResource)) Fire(chain, weapon, costResource, genResource); }
                    _pawn.Stats.mana.OnDepleted += OnManaDeplete;
                    _cleanupActions.Add(() => _pawn.Stats.mana.OnDepleted -= OnManaDeplete);
                    break;
                }
                case ReactorType.OnEnemyDeath:
                {
                    if (_target == null) break;
                    void OnEnemyDefeated() { if (ConditionMet()&& CanFire(weapon, costResource)) Fire(chain, weapon, costResource, genResource); }
                    _target.OnDefeated += OnEnemyDefeated;
                    _cleanupActions.Add(() => _target.OnDefeated -= OnEnemyDefeated);
                    break;
                }
                case ReactorType.OnAllyAttacks:
                case ReactorType.OnAllyKills:
                case ReactorType.OnNearbyEnemyDies:
                    Debug.LogWarning($"[Combat] Reactor {reactor.ReactorType} requires coordinator — not yet wired.");
                    break;
            }
        }
        
        private void Fire(IItemChain chain, IWeaponItem weapon, Resource costResource, Resource genResource)
        {
            if (_target == null) return;
            costResource.ReduceCurrent(weapon.ResourceCost);
            _target.TakeDamage(weapon.Damage);
            genResource.IncreaseCurrent(weapon.ResourceGenOnHit);
            FirePayloads(chain, weapon.Damage);
        }

        private void FirePayloads(IItemChain chain, float rootDamage)
        {
            foreach (var item in chain.Modifiers)
            {
                if (item is not IWeaponItem payload) continue;
                if (EvaluatePayloadCondition(payload, rootDamage))
                {
                    //_target?.TakeDamage(rootDamage * payload.PayloadDamageMultiplier);
                }
            }
        }

        private bool EvaluatePayloadCondition(IWeaponItem payload, float rootDamage) =>
            payload.PayloadCondition switch
            {
                ConditionType.None                  => true,
                ConditionType.ResourceFull          => _pawn.Stats.mana.IsFull,
                ConditionType.ResourceBelow         => _pawn.Stats.mana.Percentage < payload.PayloadConditionThreshold,
                ConditionType.ResourceAbove         => _pawn.Stats.mana.Percentage > payload.PayloadConditionThreshold,
                ConditionType.DamageAbove           => rootDamage >= payload.PayloadConditionThreshold,
                _                                          => false,
            };

        private bool EvaluateCondition(ConditionType type, float threshold)
        {
            return type switch
            {
                ConditionType.Always          => true,
                ConditionType.ResourceBelow   => _pawn.Stats.mana.Percentage < threshold,
                ConditionType.ResourceAbove   => _pawn.Stats.mana.Percentage > threshold,
                ConditionType.HasStatusEffect => false,
                _                                      => false,
            };
        }

        private bool CanFire(IWeaponItem weapon, Resource costResource) => costResource.CanSpend(weapon.ResourceCost);

        private static List<IShifterItem> GetActivators(IItemChain chain)
        {
            var list = new List<IShifterItem>();
            if (chain.Root is IShifterItem root) list.Add(root);
            foreach (var item in chain.Modifiers)
                if (item is IShifterItem act) list.Add(act);
            return list;
        }

        private (Resource costResource, Resource genResource) ResolveChainResources(IItemChain chain)
        {
            // Converter will override resource targets here when implemented.
            return (_pawn.Stats.mana, _pawn.Stats.mana);
        }
        
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