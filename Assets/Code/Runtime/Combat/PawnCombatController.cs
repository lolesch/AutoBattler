using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using Code.Data.Items.Weapon;
using Code.Runtime.Inventory;
using Code.Runtime.Pawns;
using Code.Runtime.Statistics;
using Submodules.Utility.Extensions;
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

        private IPawn       _target;
        private bool        _isRunning;
        private IHexContext _hexContext;

        public PawnCombatController(IPawn pawn, IHexContext hexContext = null)
        {
            _pawn       = pawn;
            _inventory  = pawn.Inventory;
            _hexContext = hexContext;
            _inventory.OnContentsChanged += _ => RebuildChains();
        }

        public void SetTarget(IPawn target)      => _target = target;
        public void SetHexContext(IHexContext context) => _hexContext = context;

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
            var shifters = GetShifters(chain);
            var (costResource, genResource) = ResolveChainResources(chain);

            foreach (var shifter in shifters)
            {
                weapon.GetInputStat(shifter.inputMod.stat).AddModifier(shifter.inputMod.modifier);
                weapon.GetOutputStat(shifter.outputMod.stat).AddModifier(shifter.inputMod.modifier);
            }

            var timer = new Timer(weapon.AttackSpeed, true);
            timer.OnRewind += () =>
            {
                timer.Duration =  1 / weapon.AttackSpeed;

                if (CanFire(weapon, costResource))
                    Fire(chain, weapon, costResource, genResource);
            };
            timer.Start();

            _cleanupActions.Add(() =>
            {
                timer.Stop();
                foreach (var shifter in shifters)
                {
                    weapon.GetInputStat(shifter.inputMod.stat).TryRemoveModifier(shifter.inputMod.modifier);
                    weapon.GetOutputStat(shifter.outputMod.stat).TryRemoveModifier(shifter.inputMod.modifier);
                }
            });
        }

        private void BuildReactor(IReactorItem reactor, IItemChain chain)
        {
            var weapon    = chain.Weapon;
            //var condType  = reactor.ConditionType;
            //var condThres = reactor.ConditionThreshold;
            var (costResource, genResource) = ResolveChainResources(chain);

            weapon.GetInputStat(reactor.inputMod.stat).AddModifier(reactor.inputMod.modifier);
            
            switch (reactor.ReactorType)
            {
                case ReactorType.OnSelfHit:
                {
                    void OnHealthChanged(float prev, float curr, float _) { if (curr < prev && CanFire(weapon, costResource)) Fire(chain, weapon, costResource, genResource); }
                    _pawn.Stats.health.OnCurrentChanged += OnHealthChanged;
                    _cleanupActions.Add(() => _pawn.Stats.health.OnCurrentChanged -= OnHealthChanged);
                    break;
                }
                case ReactorType.OnManaDeplete:
                {
                    void OnManaDeplete() { if (CanFire(weapon, costResource)) Fire(chain, weapon, costResource, genResource); }
                    _pawn.Stats.mana.OnDepleted += OnManaDeplete;
                    _cleanupActions.Add(() => _pawn.Stats.mana.OnDepleted -= OnManaDeplete);
                    break;
                }
                case ReactorType.OnEnemyDeath:
                {
                    if (_target == null) break;
                    void OnEnemyDefeated() { if (CanFire(weapon, costResource)) Fire(chain, weapon, costResource, genResource); }
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
            
            _cleanupActions.Add(() => { weapon.GetInputStat(reactor.inputMod.stat).TryRemoveModifier(reactor.inputMod.modifier); });
        }
        
        private void Fire(IItemChain chain, IWeaponItem weapon, Resource costResource, Resource genResource)
        {
            if (_target == null) return;
            costResource.ReduceCurrent(weapon.ResourceCost);
            _target.TakeDamage(weapon.Damage);
            genResource.IncreaseCurrent(weapon.ResourceGenOnHit);
            FirePayloads(chain, weapon, costResource, genResource);
        }

        private void FirePayloads(IItemChain chain, IWeaponItem rootWeapon, Resource costResource, Resource genResource)
        {
            foreach (var item in chain.Modifiers)
            {
                if (item is not IWeaponItem payload || item == rootWeapon) continue;
                if (!CanFire(payload, costResource)) continue;
                if (!EvaluatePayloadCondition(payload, (float)rootWeapon.Damage)) continue;

                var behavior = payload.Payload;
                costResource.ReduceCurrent(payload.ResourceCost);

                var (targets, hexes) = ResolvePayloadTargets(behavior);
                foreach (var target in targets)
                {
                    target.TakeDamage(payload.Damage);
                    genResource.IncreaseCurrent(payload.ResourceGenOnHit);
                }

                if (behavior != null)
                    foreach (var effect in behavior.Effects)
                        ExecuteEffect(effect, targets, hexes);
            }
        }

        private (List<IPawn> targets, List<Hex> hexes) ResolvePayloadTargets(PayloadBehavior behavior)
        {
            if (behavior == null || _target == null)
                return FallbackToSingleTarget();

            switch (behavior.Targeting)
            {
                case PayloadTargeting.Single:
                    return FallbackToSingleTarget();

                case PayloadTargeting.Self:
                    return (new List<IPawn> { _pawn }, new List<Hex> { _pawn.HexPosition });

                case PayloadTargeting.Aoe:
                    if (_hexContext == null) { LogMissingContext(); return FallbackToSingleTarget(); }
                    return (
                        _hexContext.GetPawnsInRange(_target.HexPosition, behavior.Range, PawnTeam.Enemy).ToList(),
                        _hexContext.GetHexesInRange(_target.HexPosition, behavior.Range).ToList()
                    );

                case PayloadTargeting.Line:
                    if (_hexContext == null) { LogMissingContext(); return FallbackToSingleTarget(); }
                    var lineHexes = _pawn.HexPosition.HexLine(_target.HexPosition);
                    var linePawns = new List<IPawn>();
                    foreach (var hex in lineHexes)
                        foreach (var p in _hexContext.GetPawnsInRange(hex, 0, PawnTeam.Enemy))
                            if (!linePawns.Contains(p)) linePawns.Add(p);
                    return (linePawns, lineHexes);

                default:
                    return FallbackToSingleTarget();
            }
        }

        private (List<IPawn>, List<Hex>) FallbackToSingleTarget() =>
            _target == null
                ? (new List<IPawn>(), new List<Hex>())
                : (new List<IPawn> { _target }, new List<Hex> { _target.HexPosition });

        private void ExecuteEffect(PayloadEffect effect, List<IPawn> targets, List<Hex> hexes)
        {
            switch (effect)
            {
                case StatusPayloadEffect:
                    Debug.LogWarning("[Combat] StatusPayloadEffect not yet wired — status system pending.");
                    break;
                case PositionPayloadEffect position:
                    ApplyPositionEffect(position, targets);
                    break;
                case TerrainPayloadEffect terrain:
                    ApplyTerrainEffect(terrain, hexes);
                    break;
            }
        }

        private void ApplyPositionEffect(PositionPayloadEffect effect, List<IPawn> targets)
        {
            foreach (var target in targets)
            {
                switch (effect.EffectType)
                {
                    case PositionEffectType.Push:
                    case PositionEffectType.Pull:
                        ApplyDisplacement(target, effect);
                        break;
                    case PositionEffectType.Stun:
                        Debug.LogWarning("[Combat] Stun not yet implemented.");
                        break;
                }
            }
        }

        /// <summary>
        /// Derives push/pull direction from the first step of the HexLine between this pawn and the target.
        /// Pull reverses the direction. Displacement is blocked silently if both pawns share a hex.
        /// </summary>
        private void ApplyDisplacement(IPawn target, PositionPayloadEffect effect)
        {
            var line = _pawn.HexPosition.HexLine(target.HexPosition);
            if (line.Count < 2) return; // same hex — no direction to derive

            var step = line[1].Subtract(line[0]); // unit step from pawn toward target
            if (effect.EffectType == PositionEffectType.Pull)
                step = new Hex(-step.q, -step.r); // reverse: pull toward pawn

            target.MoveTo(target.HexPosition.Add(step.Scale(effect.Distance)));
        }

        private void ApplyTerrainEffect(TerrainPayloadEffect effect, List<Hex> hexes)
        {
            if (_hexContext == null) { LogMissingContext(); return; }
            foreach (var hex in hexes)
                _hexContext.SetTerrain(hex, effect.TerrainType);
        }

        private static void LogMissingContext() =>
            Debug.LogWarning("[Combat] IHexContext not set — payload targeting falls back to single-target. Call SetHexContext before StartCombat.");

        private bool EvaluatePayloadCondition(IWeaponItem payload, float rootDamage)
        {
            var behavior = payload.Payload;
            if (behavior == null) return true;
            return behavior.Condition switch
            {
                ConditionType.None            => true,
                ConditionType.ResourceFull    => _pawn.Stats.mana.IsFull,
                ConditionType.ResourceBelow   => _pawn.Stats.mana.Percentage < behavior.ConditionThreshold,
                ConditionType.ResourceAbove   => _pawn.Stats.mana.Percentage > behavior.ConditionThreshold,
                ConditionType.DamageAbove     => rootDamage >= behavior.ConditionThreshold,
                ConditionType.HasStatusEffect => false,
                _                             => false,
            };
        }

        private bool CanFire(IWeaponItem weapon, Resource costResource) => costResource.CanSpend(weapon.ResourceCost);

        private static List<IShifterItem> GetShifters(IItemChain chain)
        {
            var list = new List<IShifterItem>();
            if (chain.Root is IShifterItem root) list.Add(root);
            foreach (var item in chain.Modifiers)
                if (item is IShifterItem shift) list.Add(shift);
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
        void SetHexContext(IHexContext context);
        void StartCombat();
        void StopCombat();
    }
}