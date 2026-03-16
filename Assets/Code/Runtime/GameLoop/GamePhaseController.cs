using System;
using Code.Data.Items;
using Code.Data.Items.Activator;
using Code.Data.Items.Amplifier;
using Code.Data.Items.Reactor;
using Code.Data.Items.Weapon;
using Code.Runtime.Inventory;
using Code.Runtime.Pawns;
using Code.Runtime.UI.Inventory;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.GameLoop
{
    public enum GamePhase { Placement, Combat, Loot }

    /// <summary>
    /// Owns the game loop state machine.
    /// Drives transitions between Placement → Combat → Loot → Placement.
    /// All per-phase logic lives in the phase classes; this is the coordinator.
    /// </summary>
    public sealed class GamePhaseController : MonoBehaviour
    {
        [Header("Pawns")]
        [SerializeField] private Pawn[] playerPawns;
        [SerializeField] private Pawn[] enemyPawns;

        [Header("Stash")]
        [SerializeField] private Vector2Int stashSize = new(8, 6);
        [Header("UI — Stash")]
        [SerializeField] private InventoryView stashView;
        
        [Header("UI — Placement")]
        [SerializeField] private Button confirmPlacementButton;

        [Header("UI — Loot")]
        [SerializeField] private Button continueAfterLootButton;

        [Header("Loot")]
        [SerializeField] private ItemConfig[] itemPool;
        [SerializeField, Min(1)] private int lootCount = 3;

        public static event Action<GamePhase> OnPhaseChanged;
        [field: SerializeField, ReadOnly] public GamePhase Current { get; private set; }

        public IPlayerData PlayerData { get; private set; }

        private IGamePhase _placementPhase;
        private IGamePhase _combatPhase;
        private IGamePhase _lootPhase;

        private void Awake()
        {
            PlayerData = new PlayerData(stashSize);
            stashView?.RefreshView(PlayerData.Stash);

            _placementPhase = new PlacementPhase(
                playerPawns,
                confirmPlacementButton,
                () => TransitionTo(GamePhase.Combat));

            _combatPhase = new CombatPhase(
                playerPawns,
                enemyPawns,
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
            OnPhaseChanged?.Invoke(Current);
            GetPhase(Current)?.Enter();
        }

        private IGamePhase GetPhase(GamePhase phase) => phase switch
        {
            GamePhase.Placement => _placementPhase,
            GamePhase.Combat    => _combatPhase,
            GamePhase.Loot      => _lootPhase,
            _                   => null,
        };
        
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