using System;
using Code.Data.Pawns;
using Code.Runtime.Combat;
using Code.Runtime.Inventory;
using Code.Runtime.Statistics;
using Code.Runtime.UI;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Runtime.Pawns
{
    public sealed class Pawn : MonoBehaviour, IPawn
    {
        [field: SerializeField] public PawnConfig Config { get; private set; }
        [field: SerializeField] public PawnTeam   Team   { get; private set; }
        [SerializeField, ReadOnly, PreviewIcon] private Sprite _icon;
        [SerializeField] private PawnEffect _pawnEffects;
        [SerializeField] private PawnResourceView  _healthView;
        [SerializeField] private PawnResourceView  _manaView;

        public IPawnStats            Stats            { get; private set; }
        public ITetrisContainer      Inventory        { get; private set; }
        public IPawnEffect           PawnEffects      { get; private set; }
        public IPawnCombatController CombatController { get; private set; }

        // Fired when this pawn is defeated. CombatPhase subscribes to track victory condition.
        public event Action OnDefeated;

        private void Awake()
        {
            SpawnPawn();
            CombatController = new PawnCombatController(Inventory);
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
            Inventory   = new TetrisContainer(new Vector2Int(6, 3), Stats);
            PawnEffects = _pawnEffects;

            Stats.health.OnDepleted += DespawnPawn;
            _healthView.SetPawn(Stats.health);
            _manaView.SetPawn(Stats.mana);
        }

        private void DespawnPawn()
        {
            Debug.Log($"{gameObject.name} has been defeated!");
            OnDefeated?.Invoke();
            gameObject.SetActive(false);
        }

        public void TakeDamage(float damage) => Stats.health.ReduceCurrent(damage);

        public void EquipItem(TetrisItem item)
        {
            if (Inventory.TryAdd(item))
                Debug.Log($"{item.Name} has been equipped!");
        }
    }

    public enum PawnTeam { Player, Enemy }

    public interface IPawn : IDamageable
    {
        IPawnStats            Stats            { get; }
        ITetrisContainer      Inventory        { get; }
        IPawnEffect           PawnEffects      { get; }
        IPawnCombatController CombatController { get; }
        PawnTeam              Team             { get; }
        event Action          OnDefeated;
        void EquipItem(TetrisItem item);
    }

    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}