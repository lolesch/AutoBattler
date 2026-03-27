using System;
using System.Collections.Generic;
using Code.Data.Enums;
using Code.Data.Items.Activator;
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
                ChainResolver.LogChain(chain);

                switch (chain.Root)
                {
                    case IWeaponItem:
                    case IActivatorItem:
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
            var applied    = new Dictionary<IActivatorItem, (Modifier firing, Modifier output)>();

            EvaluateActivators();

            var timer = new Timer(weapon.AttackSpeed, true);
            timer.OnRewind += () =>
            {
                EvaluateActivators();
                timer.Duration = weapon.AttackSpeed;

                if (CanFire(weapon))
                    Fire(chain, weapon);
            };
            timer.Start();

            _cleanupActions.Add(() =>
            {
                timer.Stop();
                foreach (var (act, (firingMod, outputMod)) in applied)
                {
                    WeaponUtils.GetFiringStat(weapon, act.FiringStat).TryRemoveModifier(firingMod);
                    if (act.OutputValue != 0)
                        WeaponUtils.GetOutputStat(weapon, act.OutputStat).TryRemoveModifier(outputMod);
                }
                applied.Clear();
            });
            return;

            void EvaluateActivators()
            {
                foreach (var act in activators)
                {
                    var firingStat = WeaponUtils.GetFiringStat(weapon, act.FiringStat);
                    var outputStat = WeaponUtils.GetOutputStat(weapon,  act.OutputStat);
                    var conditionMet = EvaluateCondition(act.ConditionType, act.ConditionThreshold);
                    var isApplied    = applied.ContainsKey(act);

                    if (conditionMet && !isApplied)
                    {
                        var firingMod = new Modifier( act.FiringValue, act.FiringModifierType, act.Guid);
                        var outputMod = new Modifier( act.OutputValue, act.OutputModifierType, act.Guid);
                        firingStat.AddModifier(firingMod);
                        if (act.OutputValue != 0)
                            outputStat.AddModifier(outputMod);
                        applied[act] = (firingMod, outputMod);
                    }
                    else if (!conditionMet && isApplied)
                    {
                        var (firingMod, outputMod) = applied[act];
                        firingStat.TryRemoveModifier(firingMod);
                        if (act.OutputValue != 0)
                            outputStat.TryRemoveModifier(outputMod);
                        applied.Remove(act);
                    }
                }
            }
        }

        private void BuildReactor(IReactorItem reactor, IItemChain chain)
        {
            var weapon    = chain.Weapon;
            var condType  = reactor.ConditionType;
            var condThres = reactor.ConditionThreshold;

            bool ConditionMet() => EvaluateCondition(condType, condThres);

            switch (reactor.ReactorType)
            {
                case ReactorType.OnSelfHit:
                {
                    void OnHealthChanged(float prev, float curr, float _)
                    { if (curr < prev && ConditionMet()) Fire(chain, weapon); }
                    _pawn.Stats.health.OnCurrentChanged += OnHealthChanged;
                    _cleanupActions.Add(() => _pawn.Stats.health.OnCurrentChanged -= OnHealthChanged);
                    break;
                }
                case ReactorType.OnManaDeplete:
                {
                    void OnManaDeplete() { if (ConditionMet()) Fire(chain, weapon); }
                    _pawn.Stats.mana.OnDepleted += OnManaDeplete;
                    _cleanupActions.Add(() => _pawn.Stats.mana.OnDepleted -= OnManaDeplete);
                    break;
                }
                case ReactorType.OnEnemyDeath:
                {
                    if (_target == null) break;
                    void OnEnemyDefeated() { if (ConditionMet()) Fire(chain, weapon); }
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
        
        private void Fire(IItemChain chain, IWeaponItem weapon)
        {
            if (_target == null) return;
            _target.TakeDamage(weapon.Damage);
            // TODO: spend ResourceCost from _pawn.Stats.mana
            // TODO: apply ResourceGenOnHit to _pawn.Stats.mana
            FirePayloads(chain, weapon.Damage);
        }

        private void FirePayloads(IItemChain chain, float rootDamage)
        {
            foreach (var item in chain.Modifiers)
            {
                if (item is not IWeaponItem payload) continue;
                if (EvaluatePayloadCondition(payload, rootDamage))
                    _target?.TakeDamage(rootDamage * payload.PayloadDamageMultiplier);
            }
        }

        private bool EvaluatePayloadCondition(IWeaponItem payload, float rootDamage) =>
            payload.PayloadCondition switch
            {
                PayloadConditionType.None                  => true,
                PayloadConditionType.HealthBelow           => _pawn.Stats.health.Percentage < payload.PayloadConditionThreshold,
                PayloadConditionType.HealthAbove           => _pawn.Stats.health.Percentage > payload.PayloadConditionThreshold,
                PayloadConditionType.ResourceFull          => _pawn.Stats.mana.IsFull,
                PayloadConditionType.ResourceBelow         => _pawn.Stats.mana.Percentage < payload.PayloadConditionThreshold,
                PayloadConditionType.ResourceAbove         => _pawn.Stats.mana.Percentage > payload.PayloadConditionThreshold,
                PayloadConditionType.RootDamageAbove       => rootDamage >= payload.PayloadConditionThreshold,
                PayloadConditionType.RootKilledTarget      => _target.Stats.health.IsDepleted,
                PayloadConditionType.TargetHealthBelow     => _target.Stats.health.Percentage < payload.PayloadConditionThreshold,
                PayloadConditionType.TargetHealthAbove     => _target.Stats.health.Percentage > payload.PayloadConditionThreshold,
                PayloadConditionType.TargetHasStatusEffect => false,
                _                                          => false,
            };

        private bool EvaluateCondition(ActivatorConditionType type, float threshold)
        {
            return type switch
            {
                ActivatorConditionType.Always          => true,
                ActivatorConditionType.HpBelow         => _pawn.Stats.health.Percentage < threshold,
                ActivatorConditionType.HpAbove         => _pawn.Stats.health.Percentage > threshold,
                ActivatorConditionType.ResourceBelow   => _pawn.Stats.mana.Percentage < threshold,
                ActivatorConditionType.ResourceAbove   => _pawn.Stats.mana.Percentage > threshold,
                ActivatorConditionType.FirstXSeconds   => false,
                ActivatorConditionType.EnemyCountBelow => false,
                ActivatorConditionType.AllyCountBelow  => false,
                ActivatorConditionType.HasStatusEffect => false,
                _                                      => false,
            };
        }

        private bool CanFire(IWeaponItem weapon) =>
            weapon.ResourceCost <= 0 || _pawn.Stats.mana.CanSpend(weapon.ResourceCost);

        private static List<IActivatorItem> GetActivators(IItemChain chain)
        {
            var list = new List<IActivatorItem>();
            if (chain.Root is IActivatorItem root) list.Add(root);
            foreach (var item in chain.Modifiers)
                if (item is IActivatorItem act) list.Add(act);
            return list;
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