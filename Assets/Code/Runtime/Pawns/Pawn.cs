using System;
using Code.Data.Enums;
using Code.Data.Pawns;
using Code.Runtime.GameLoop;
using Code.Runtime.HexGrid;
using Code.Runtime.Inventory;
using Code.Runtime.Statistics;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.Pawns
{
    public sealed class Pawn : MonoBehaviour, IPawn, ICombatParticipant, IHexOccupant
    {
        [field: SerializeField] public PawnConfig        Config { get; private set; }
        [field: SerializeField] public PawnTeam          Team   { get; private set; }
        [field: SerializeField] public Hex               HexPosition      { get; private set; }
        [field: SerializeField] public HexGridController HexGrid { get; private set; }
        [field: SerializeField] public GamePhaseController GameLoop { get; private set; }

        public void UnregisterFrom(HexGridController controller) => controller.Unregister(this);
        public void RegisterWith(HexGridController controller) => controller.Register(this);
        public void UnregisterFrom(GamePhaseController controller) => controller.Unregister(this);
        public void RegisterWith(GamePhaseController controller) => controller.Register(this);

        [SerializeField, ReadOnly, PreviewIcon] private Sprite _icon;
        [SerializeField] private PawnEffect _pawnEffects;

        public IPawnStats            Stats            { get; private set; }
        public ITetrisContainer      Inventory        { get; private set; }
        public IPawnEffect           PawnEffects      { get; private set; }
        public IPawnCombatController CombatController { get; private set; }

        public event Action OnDefeated;

        private void Awake()
        {
            SpawnPawn();
            CombatController = new PawnCombatController(this);
            //_chainStateController = new ChainStateController(Inventory, Stats);
        }

        [ContextMenu("Spawn")]
        private void SpawnPawn()
        {
            if (!Config)
            {
                Debug.LogError("Missing Config to draw from");
                return;
            }

            _icon       = Config.icon;
            Stats       = new PawnStats(Config);
            Inventory   = new TetrisContainer(new Vector2Int(6, 3));
            PawnEffects = _pawnEffects;
            
            if (Config.StarterWeapon != null)
                Inventory.TryAdd(ItemFactory.Create(Config.StarterWeapon));
            else
                Debug.LogWarning($"{gameObject.name} has no StarterWeapon assigned in PawnConfig.", this);

            Stats.health.OnDepleted += DespawnPawn;
            //_healthView.SetPawn(Stats.health);
            //_manaView.SetPawn(Stats.mana);
            RegisterWith(HexGrid);
            RegisterWith(GameLoop);
        }

        private void DespawnPawn()
        {
            Debug.Log($"{gameObject.name} has been defeated!");
            OnDefeated?.Invoke();
            gameObject.SetActive(false);
            UnregisterFrom(HexGrid);
            UnregisterFrom(GameLoop);
        }

        public void TakeDamage(float damage) => Stats.health.ReduceCurrent(damage);
        public void MoveTo(Hex hex)
        {
            HexPosition = hex;
        }

        public void EquipItem(TetrisItem item)
        {
            if (Inventory.TryAdd(item))
                Debug.Log($"{item.Name} has been equipped!");
        }
    }

    public interface IPawn : IDamageable
    {
        IPawnStats            Stats            { get; }
        ITetrisContainer      Inventory        { get; }
        IPawnEffect           PawnEffects      { get; }
        IPawnCombatController CombatController { get; }
        event Action          OnDefeated;
        void MoveTo(Hex hex);
        void EquipItem(TetrisItem item);
    }

    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}