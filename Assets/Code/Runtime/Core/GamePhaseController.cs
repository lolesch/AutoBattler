using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using Code.Data.Items;
using Code.Runtime.Core.Combat;
using Code.Runtime.Modules.Inventory;
using Code.Runtime.Pawns;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.Core
{
    /// <summary>
    /// Owns the game loop state machine.
    /// Drives transitions between Placement → Combat → Loot → Placement.
    /// All per-phase logic lives in the phase classes; this is the coordinator.
    /// </summary>
    public sealed class GamePhaseController : MonoBehaviour
    {
        [Header("Pawns")]
        [SerializeField] private static List<IPawn> _enemyPawns  = new();
        [SerializeField] private static List<IPawn> _playerPawns = new();
        //TODO: probably want to change this lookup
        public static IEnumerable<IPawn> allPawns => _enemyPawns.Concat(_playerPawns);

        [Header("Combat")]
        [SerializeField] private CombatCoordinator combatCoordinator;

        [Header("Stash")]
        [SerializeField] private Vector2Int stashSize = new(8, 6);

        [Header("UI — Placement")]
        [SerializeField] private Button confirmPlacementButton;

        [Header("UI — Loot")]
        [SerializeField] private Button continueAfterLootButton;

        [Header("Loot")]
        [SerializeField] private ItemConfig[] itemPool;
        [SerializeField, Min(1)] private int lootCount = 3;

        [field: SerializeField, ReadOnly] public GamePhase Current { get; private set; }

        public IPlayerData PlayerData { get; private set; }

        private IGamePhase _placementPhase;
        private IGamePhase _combatPhase;
        private IGamePhase _lootPhase;

        private void Awake()
        {
            PlayerData = new PlayerData(stashSize);

            _placementPhase = new PlacementPhase(
                _playerPawns,
                confirmPlacementButton,
                () => TransitionTo(GamePhase.Combat));

            _combatPhase = new CombatPhase(
                combatCoordinator,
                () => TransitionTo(GamePhase.Loot));

            _lootPhase = new LootPhase(
                PlayerData,
                itemPool,
                lootCount,
                continueAfterLootButton,
                () => TransitionTo(GamePhase.Placement));
        }

        private void Start()
        {
            TransitionTo(GamePhase.Placement);
            AddItems();
        }

        public void TransitionTo(GamePhase next)
        {
            GetPhase(Current)?.Exit();
            Current = next;
            GetPhase(Current)?.Enter();
        }

        private IGamePhase GetPhase(GamePhase phase) => phase switch
        {
            GamePhase.Placement => _placementPhase,
            GamePhase.Combat    => _combatPhase,
            GamePhase.Loot      => _lootPhase,
            _                   => null,
        };

        public void Register(IPawn pawn)
        {
            if (pawn.Team == PawnTeam.Enemy  && !_enemyPawns.Contains(pawn))  _enemyPawns.Add(pawn);
            if (pawn.Team == PawnTeam.Player && !_playerPawns.Contains(pawn)) _playerPawns.Add(pawn);
        }

        public void Unregister(IPawn pawn)
        {
            if (pawn.Team == PawnTeam.Enemy)  _enemyPawns.Remove(pawn);
            if (pawn.Team == PawnTeam.Player) _playerPawns.Remove(pawn);
        }

        [ContextMenu("AddItems")]
        private void AddItems()
        {
            foreach (var config in itemPool)
            {
                PlayerData.Stash.TryAdd(ItemFactory.Create(config));
                PlayerData.Stash.TryAdd(ItemFactory.Create(config));
            }
        }
    }

    public interface IGamePhase
    {
        void Enter();
        void Exit();
    }
}