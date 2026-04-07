using System;
using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using Code.Data.Items.Weapon;
using Code.Runtime.Modules.HexGrid;
using Code.Runtime.Modules.Inventory;
using Code.Runtime.Modules.Statistics;
using Code.Runtime.Pawns;
using Submodules.Utility.Extensions;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime.Core.Combat
{
    /// <summary>
    /// Owns all combat state for a single pawn.
    /// Created and managed exclusively by CombatCoordinator — never self-owned by Pawn.
    /// Weapon stats are live MutableFloat objects — amplifier mods applied once per chain
    /// build, all cleanup routes through _cleanupActions.
    /// </summary>
    public sealed class PawnCombatController : IPawnCombatController
    {
        private readonly IPawn      _pawn;
        private readonly ITetrisContainer _inventory;
        private readonly IHexGrid         _hexGrid;
        private readonly ICombatEventBus  _eventBus;
        private readonly List<Action>     _cleanupActions = new();

        private IPawn _target;
        private bool        _isRunning;

        public PawnCombatController(IPawn pawn, IHexGrid hexGrid, ICombatEventBus eventBus)
        {
            _pawn      = pawn;
            _inventory = pawn.Inventory;
            _hexGrid   = hexGrid;
            _eventBus  = eventBus;

            _inventory.OnContentsChanged += _ => RebuildChains();
        }

        public void SetCurrentTarget(IPawn target) => _target = target;

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

        private void BuildTimedChain(IItemChain chain)
        {
            var weapon  = chain.Weapon;
            var shifters = GetShifters(chain);
            var (costResource, genResource) = ResolveChainResources(chain);

            foreach (var shifter in shifters)
            {
                weapon.GetInputStat(shifter.inputMod.stat).AddModifier(shifter.inputMod.modifier);
                weapon.GetOutputStat(shifter.outputMod.stat).AddModifier(shifter.inputMod.modifier);
            }

            var timer = new Timer(1f / weapon.AttackSpeed, true);
            timer.OnRewind += () =>
            {
                timer.Duration = 1f / weapon.AttackSpeed;
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
            var weapon = chain.Weapon;
            var (costResource, genResource) = ResolveChainResources(chain);

            weapon.GetInputStat(reactor.inputMod.stat).AddModifier(reactor.inputMod.modifier);

            switch (reactor.ReactorType)
            {
                case ReactorType.OnSelfHit:
                {
                    void OnHealthChanged(float prev, float curr, float _)
                    {
                        if (curr < prev && CanFire(weapon, costResource))
                            Fire(chain, weapon, costResource, genResource);
                    }
                    _pawn.Stats.health.OnCurrentChanged += OnHealthChanged;
                    _cleanupActions.Add(() => _pawn.Stats.health.OnCurrentChanged -= OnHealthChanged);
                    break;
                }
                case ReactorType.OnManaDeplete:
                {
                    void OnManaDeplete()
                    {
                        if (CanFire(weapon, costResource))
                            Fire(chain, weapon, costResource, genResource);
                    }
                    _pawn.Stats.mana.OnDepleted += OnManaDeplete;
                    _cleanupActions.Add(() => _pawn.Stats.mana.OnDepleted -= OnManaDeplete);
                    break;
                }
                case ReactorType.OnEnemyDeath:
                {
                    void OnDefeated(IPawn unit)
                    {
                        if (unit.Team == _pawn.Team) return;
                        if (CanFire(weapon, costResource))
                            Fire(chain, weapon, costResource, genResource);
                    }
                    _eventBus.OnUnitDefeated += OnDefeated;
                    _cleanupActions.Add(() => _eventBus.OnUnitDefeated -= OnDefeated);
                    break;
                }
                case ReactorType.OnAllyAttacks:
                {
                    void OnAllyAttacked(IPawn unit)
                    {
                        if (unit.Team != _pawn.Team || unit == _pawn) return;
                        if (CanFire(weapon, costResource))
                            Fire(chain, weapon, costResource, genResource);
                    }
                    _eventBus.OnUnitAttacked += OnAllyAttacked;
                    _cleanupActions.Add(() => _eventBus.OnUnitAttacked -= OnAllyAttacked);
                    break;
                }
                case ReactorType.OnAllyKills:
                {
                    void OnAllyKill(IPawn unit)
                    {
                        if (unit.Team != _pawn.Team || unit == _pawn) return;
                        if (CanFire(weapon, costResource))
                            Fire(chain, weapon, costResource, genResource);
                    }
                    // OnAllyKills = ally attacks that result in a defeat.
                    // The kill event isn't separately tracked; subscribe to OnUnitDefeated
                    // and check if the last attacker was an ally — not yet tracked on the bus.
                    // TODO: add ICombatEventBus.OnUnitKilled(killer, victim) when kill attribution is needed.
                    Debug.LogWarning("[Combat] OnAllyKills reactor wired to OnUnitDefeated without kill attribution — extend ICombatEventBus when ready.");
                    _eventBus.OnUnitDefeated += OnAllyKill;
                    _cleanupActions.Add(() => _eventBus.OnUnitDefeated -= OnAllyKill);
                    break;
                }
                case ReactorType.OnNearbyEnemyDies:
                {
                    void OnNearbyEnemyDefeated(IPawn unit)
                    {
                        if (unit.Team == _pawn.Team) return;
                        if (_hexGrid == null) return;
                        if (_pawn.HexPosition.Distance(unit.HexPosition) > /*reactor.Range*/ 1) return;
                        if (CanFire(weapon, costResource))
                            Fire(chain, weapon, costResource, genResource);
                    }
                    _eventBus.OnUnitDefeated += OnNearbyEnemyDefeated;
                    _cleanupActions.Add(() => _eventBus.OnUnitDefeated -= OnNearbyEnemyDefeated);
                    break;
                }
            }

            _cleanupActions.Add(() => weapon.GetInputStat(reactor.inputMod.stat).TryRemoveModifier(reactor.inputMod.modifier));
        }

        private void Fire(IItemChain chain, IWeaponItem weapon, Resource costResource, Resource genResource)
        {
            if (_target == null) return;
            costResource.ReduceCurrent(weapon.ResourceCost);
            _target.TakeDamage(weapon.Damage);
            genResource.IncreaseCurrent(weapon.ResourceGenOnHit);

            _eventBus.PublishAttacked(_pawn);
            _eventBus.PublishHit(_pawn, _target);

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
                    _eventBus.PublishHit(_pawn, target);
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
                {
                    var aoeTargets = TargetSelector
                        .GetPawnsInRange(_target.HexPosition, behavior.Range, CombatCoordinator.allUnits, PawnTeam.Enemy).ToList();
                    return aoeTargets.Count > 0 ? (aoeTargets, _target.HexPosition.HexRange(behavior.Range)) : FallbackToSingleTarget();
                }

                case PayloadTargeting.Line:
                {
                    if (_hexGrid == null) { LogMissingHexGrid(); return FallbackToSingleTarget(); }
                    var lineHexes  = _pawn.HexPosition.HexLine(_target.HexPosition);
                    var lineTargets = new List<IPawn>();
                    foreach (var hex in lineHexes)
                        foreach (var occupant in TargetSelector.GetPawnsInRange(hex, 0, CombatCoordinator.allUnits, PawnTeam.Enemy))
                            if (occupant is IPawn pawn && !lineTargets.Contains(pawn))
                                lineTargets.Add(pawn);
                    return (lineTargets, lineHexes);
                }

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

        private void ApplyDisplacement(IPawn target, PositionPayloadEffect effect)
        {
            var line = _pawn.HexPosition.HexLine(target.HexPosition);
            if (line.Count < 2) return;

            var step = line[1].Subtract(line[0]);
            if (effect.EffectType == PositionEffectType.Pull)
                step = new Hex(-step.q, -step.r);

            target.MoveTo(target.HexPosition.Add(step.Scale(effect.Distance)));
        }

        private void ApplyTerrainEffect(TerrainPayloadEffect effect, List<Hex> hexes)
        {
            if (_hexGrid == null) { LogMissingHexGrid(); return; }
            foreach (var hex in hexes)
                _hexGrid.SetTerrain(hex, effect.TerrainType);
        }

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

        private bool CanFire(IWeaponItem weapon, Resource costResource) =>
            costResource.CanSpend(weapon.ResourceCost);

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

        private static void LogMissingHexGrid() =>
            Debug.LogWarning("[Combat] IHexGrid not set — payload targeting falls back to single-target.");

        private void Cleanup()
        {
            foreach (var action in _cleanupActions) action();
            _cleanupActions.Clear();
        }
    }

    public interface IPawnCombatController
    {
        void SetCurrentTarget(IPawn target);
        void StartCombat();
        void StopCombat();
    }
}