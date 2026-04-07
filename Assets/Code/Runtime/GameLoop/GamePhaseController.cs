using System.Collections.Generic;
using System.Linq;
using Code.Data.Enums;
using Code.Data.Items;
using Code.Runtime.Inventory;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.GameLoop
{
    /// <summary>
    /// Owns the game loop state machine.
    /// Drives transitions between Placement → Combat → Loot → Placement.
    /// All per-phase logic lives in the phase classes; this is the coordinator.
    /// </summary>
    public sealed class GamePhaseController : MonoBehaviour
    {

        public static List<ICombatParticipant> enemyPawns { get; set; }
        public static List<ICombatParticipant> playerPawns { get; set; }
        
        [Header("Stash")]
        [SerializeField] private Vector2Int stashSize = new(8, 6);
        [Header("UI — Stash")]
        //[SerializeField] private InventoryView stashView;
        
        [Header("UI — Placement")]
        [SerializeField] private Button confirmPlacementButton;

        [Header("UI — Loot")]
        [SerializeField] private Button continueAfterLootButton;

        [Header("Loot")]
        [SerializeField] private ItemConfig[] itemPool;
        [SerializeField, Min(1)] private int lootCount = 3;

        [field: SerializeField, ReadOnly] public GamePhase Current { get; private set; }

        public IPlayerData PlayerData { get; private set; }
        public static IEnumerable<ICombatParticipant> allPawns => enemyPawns.Concat(playerPawns);

        private IGamePhase _placementPhase;
        private IGamePhase _combatPhase;
        private IGamePhase _lootPhase;

        private void Awake()
        {
            PlayerData = new PlayerData(stashSize);
            //stashView?.RefreshView(PlayerData.Stash);

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
            GetPhase(Current)?.Enter();
            
            ////TODO: move OnPhaseChanged into pawn and let Draggable look that up
            //foreach (var pawn in playerPawns)
            //    pawn.GetComponent<Draggable>().OnPhaseChanged(Current);
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

        public void Register(ICombatParticipant pawn)
        {
            if (pawn.Team == PawnTeam.Enemy && !enemyPawns.Contains(pawn)) 
                enemyPawns.Add(pawn);
            else if (pawn.Team == PawnTeam.Player && !playerPawns.Contains(pawn)) 
                playerPawns.Add(pawn);
        }

        public void Unregister(ICombatParticipant pawn)
        {
            if (pawn.Team == PawnTeam.Enemy) enemyPawns.Remove(pawn);
            else if (pawn.Team == PawnTeam.Player) playerPawns.Remove(pawn);
        }
    }

    public interface IGamePhase
    {
        void Enter();
        void Exit();
    }
}