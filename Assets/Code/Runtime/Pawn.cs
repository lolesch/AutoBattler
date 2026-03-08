using Code.Data.Pawns;
using Code.Runtime.Container;
using Code.Runtime.Container.Items;
using Code.Runtime.Grids;
using Code.Runtime.Statistics;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Runtime
{
    public sealed class Pawn : MonoBehaviour, IPawn
    {
        [SerializeField] private PawnConfig _config;
        [SerializeField, ReadOnly, PreviewIcon] private Sprite _icon;
        [SerializeField] private PawnEffect _pawnEffects;

        public IPawnStats            Stats            { get; private set; }
        public ITetrisContainer      Inventory        { get; private set; }
        public IPawnEffect           PawnEffects      { get; private set; }
        public IPawnCombatController CombatController { get; private set; }

        private void Awake()
        {
            SpawnPawn();
            CombatController = new PawnCombatController(Inventory);
        }

        [ContextMenu("Spawn")]
        private void SpawnPawn()
        {
            if (!_config)
            {
                Debug.LogError("Missing Config to draw from");
                return;
            }

            _icon       = _config.icon;
            Stats       = new PawnStats(_config);
            Inventory   = new TetrisContainer(new Vector2Int(6, 3), Stats);
            PawnEffects = _pawnEffects;

            Stats.health.OnDepleted += DespawnPawn;
        }

        private void DespawnPawn()
        {
            Debug.Log($"{gameObject.name} has been defeated!");
            gameObject.SetActive(false);
        }

        public void TakeDamage(float damage) => Stats.health.ReduceCurrent(damage);
        public void EquipItem(TetrisItem item)
        {
            if( Inventory.TryAdd(item) )
                Debug.Log($"{item.Name} has been equipped!");
        }
    }

    public interface IPawn : IDamageable
    {
        IPawnStats            Stats            { get; }
        ITetrisContainer      Inventory        { get; }
        IPawnEffect           PawnEffects      { get; }
        IPawnCombatController CombatController { get; }
        void EquipItem(TetrisItem item);
    }

    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}