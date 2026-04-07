using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using Code.Runtime.Modules.HexGrid;
using Code.Runtime.Modules.Inventory;
using Code.Runtime.Pawns;
using Submodules.Utility.Extensions;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime.Core.Combat
{
    /// <summary>
    /// Scene-level authority for all combat.
    /// Owns PawnCombatController lifetimes, per-unit targeting, hex reservation,
    /// and movement timers. CombatPhase calls StartCombat / StopCombat.
    /// Pawns self-register via Register / Unregister.
    /// </summary>
    public sealed class CombatCoordinator : MonoBehaviour, ICombatCoordinator
    {
        [SerializeField] private HexGridController _hexGrid;

        private static readonly List<IPawn> _playerUnits = new();
        private static readonly List<IPawn> _enemyUnits  = new();
        public static IEnumerable<IPawn> allUnits => _playerUnits.Concat(_enemyUnits);

        // Per-unit combat controllers
        private readonly Dictionary<IPawn, PawnCombatController> _controllers    = new();
        // Where each unit currently stands (updated on MoveTo)
        private readonly Dictionary<IPawn, Hex>                  _claimedHexes   = new();
        // Destination reserved by a unit that is currently mid-movement
        private readonly Dictionary<IPawn, Hex>                  _reservedHexes  = new();
        private readonly Dictionary<IPawn, ITimer>               _movementTimers = new();
        // Max weapon range per unit — derived from chains, refreshed on RebuildChains
        private readonly Dictionary<IPawn, int>                  _maxWeaponRange = new();

        private ICombatEventBus _eventBus;
        private bool            _isRunning;

        private void Awake()
        {
            _eventBus       = new CombatEventBus();
        }

        // ── Registration ─────────────────────────────────────────────────

        public void Register(IPawn unit)
        {
            var list = unit.Team == PawnTeam.Player ? _playerUnits : _enemyUnits;
            if (!list.Contains(unit)) list.Add(unit);
        }

        public void Unregister(IPawn unit)
        {
            _playerUnits.Remove(unit);
            _enemyUnits.Remove(unit);
        }

        // ── Lifecycle ────────────────────────────────────────────────────

        public void StartCombat()
        {
            _isRunning = true;

            foreach (var unit in _playerUnits) InitUnit(unit);
            foreach (var unit in _enemyUnits)  InitUnit(unit);

            foreach (var unit in _playerUnits) EvaluateUnit(unit, _enemyUnits);
            foreach (var unit in _enemyUnits)  EvaluateUnit(unit, _playerUnits);
        }

        public void StopCombat()
        {
            _isRunning = false;

            foreach (var (_, controller) in _controllers)  controller.StopCombat();
            foreach (var (_, timer)      in _movementTimers) timer.Stop();

            _controllers.Clear();
            _movementTimers.Clear();
            _claimedHexes.Clear();
            _reservedHexes.Clear();
            _maxWeaponRange.Clear();
        }

        // ── Internal ─────────────────────────────────────────────────────

        private void InitUnit(IPawn unit)
        {
            _claimedHexes[unit]   = unit.HexPosition;
            _maxWeaponRange[unit] = ResolveMaxRange(unit);
            _controllers[unit]    = new PawnCombatController(unit, _hexGrid, _eventBus);

            unit.OnDefeated += () => OnUnitDefeated(unit);
        }

        /// <summary>
        /// Core per-unit decision: find a target or move toward one.
        /// Called on combat start, on each hex arrival, and on any defeat.
        /// </summary>
        private void EvaluateUnit(IPawn unit, IReadOnlyList<IPawn> opponents)
        {
            if (!_isRunning) return;

            var target = TargetSelector.Select(unit, opponents, _maxWeaponRange[unit]);
            var controller = _controllers[unit];

            if (target != null)
            {
                StopMovement(unit);
                controller.SetCurrentTarget(target);
                controller.StartCombat();
            }
            else
            {
                controller.StopCombat();
                StartMovement(unit, opponents);
            }
        }

        private void StartMovement(IPawn unit, IReadOnlyList<IPawn> opponents)
        {
            if (_movementTimers.ContainsKey(unit)) return;

            var nearest = FindNearest(unit, opponents);
            if (nearest == null) return;

            ScheduleNextStep(unit, nearest, opponents);
        }

        private void StopMovement(IPawn unit)
        {
            if (!_movementTimers.TryGetValue(unit, out var timer)) return;
            timer.Stop();
            _movementTimers.Remove(unit);
            _reservedHexes.Remove(unit);
        }

        private void ScheduleNextStep(IPawn unit, IPawn destination, IReadOnlyList<IPawn> opponents)
        {
            var nextHex = ResolveNextHex(unit, destination);
            if (nextHex == Hex.Invalid) return;

            // Reserve before the timer fires — other units see it immediately.
            _reservedHexes[unit] = nextHex;

            var terrainType = _hexGrid.GetTerrain(nextHex);
            var moveCost    = unit.MovementCosts?.GetCost(terrainType) ?? 1;
            var duration    = moveCost / Mathf.Max(unit.Stats.movementSpeed, 0.01f);

            var timer = new Timer(duration, false);
            _movementTimers[unit] = timer;

            timer.OnRewind += () =>
            {
                _movementTimers.Remove(unit);
                _reservedHexes.Remove(unit);
                _claimedHexes[unit] = nextHex;
                unit.MoveTo(nextHex);
                EvaluateUnit(unit, opponents);
            };
            timer.Start();
        }

        /// <summary>
        /// Builds invalid and occupied sets, runs A*, and returns the first step hex.
        /// </summary>
        private Hex ResolveNextHex(IPawn unit, IPawn destination)
        {
            var (invalidSet, occupiedSet) = BuildPathingSets(unit);

            var path = HexPathfinder.FindPath(
                unit.HexPosition,
                destination.HexPosition,
                invalidSet,
                _hexGrid,
                occupiedSet,
                unit.MovementCosts);

            if (path == null) return Hex.Invalid;

            // Walk the parent chain back to find the first step after the start.
            var node = path;
            while (node.Parent != null && node.Parent.Hex != unit.HexPosition)
                node = node.Parent;

            return node.Hex == unit.HexPosition ? Hex.Invalid : node.Hex;
        }

        /// <summary>
        /// <para>invalidSet — impassable as destination AND as traversal node (e.g. terrain walls).
        /// Currently only reserved destinations from other mid-movement units.</para>
        /// <para>occupiedSet — high-cost traversal (units currently standing still).
        /// Passable in traversal but never valid as a landing hex.</para>
        /// </summary>
        private (HashSet<Hex> invalidSet, HashSet<Hex> occupiedSet) BuildPathingSets(IPawn movingUnit)
        {
            // Other units' destinations are invalid to land on.
            var invalidSet = new HashSet<Hex>();
            foreach (var (unit, hex) in _reservedHexes)
                if (unit != movingUnit) invalidSet.Add(hex);

            // Units standing still are expensive to pass through, never valid to land on.
            var occupiedSet = new HashSet<Hex>();
            foreach (var (unit, hex) in _claimedHexes)
                if (unit != movingUnit) occupiedSet.Add(hex);

            // Occupied hexes are also invalid as destinations.
            invalidSet.UnionWith(occupiedSet);

            return (invalidSet, occupiedSet);
        }

        private IPawn FindNearest(IPawn unit, IReadOnlyList<IPawn> candidates)
        {
            IPawn nearest  = null;
            var         bestDist = int.MaxValue;

            foreach (var candidate in candidates)
            {
                var dist = unit.HexPosition.Distance(candidate.HexPosition);
                if (dist >= bestDist) continue;
                bestDist = dist;
                nearest  = candidate;
            }

            return nearest;
        }

        private void OnUnitDefeated(IPawn unit)
        {
            StopMovement(unit);

            if (_controllers.TryGetValue(unit, out var controller))
            {
                controller.StopCombat();
                _controllers.Remove(unit);
            }

            _claimedHexes.Remove(unit);
            _reservedHexes.Remove(unit);
            _maxWeaponRange.Remove(unit);
            _playerUnits.Remove(unit);
            _enemyUnits.Remove(unit);

            _eventBus.PublishDefeated(unit);

            // Re-evaluate all surviving units — their target may be gone or a new gap opened.
            foreach (var u in _playerUnits) EvaluateUnit(u, _enemyUnits);
            foreach (var u in _enemyUnits)  EvaluateUnit(u, _playerUnits);
        }

        /// <summary>
        /// Derives max weapon range from the unit's current chains.
        /// TODO: requires IWeaponItem.Range to be implemented. Defaults to 1 (melee) until then.
        /// </summary>
        private static int ResolveMaxRange(IPawn unit)
        {
            var chains   = ChainResolver.Resolve(unit.Inventory);
            var maxRange = 1;
            foreach (var chain in chains)
                // TODO: revise range!
                if (chain.Weapon.Payload.Range > maxRange)
                    maxRange = chain.Weapon.Payload.Range;
            return maxRange;
        }
    }

    public interface ICombatCoordinator
    {
        void Register(IPawn unit);
        void Unregister(IPawn unit);
        void StartCombat();
        void StopCombat();
    }
}