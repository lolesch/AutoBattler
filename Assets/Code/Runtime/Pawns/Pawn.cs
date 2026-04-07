using System;
using Code.Data.Enums;
using Code.Data.Pawns;
using Code.Runtime.Modules.HexGrid;
using Code.Runtime.Modules.Inventory;
using Code.Runtime.Modules.Statistics;
using NaughtyAttributes;
using Submodules.Utility.Attributes;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.Pawns
{
    public sealed class Pawn : MonoBehaviour, IPawn
    {
        [SerializeField, ReadOnly, PreviewIcon] private Sprite _icon;
        [SerializeField] private PawnEffect _pawnEffects;

        [field: SerializeField] public PawnConfig        Config        { get; private set; }
        [field: SerializeField] public PawnTeam          Team          { get; private set; }
        [field: SerializeField] public Hex               HexPosition   { get; private set; }
        
        public IPawnStats       Stats         { get; private set; }
        public ITetrisContainer Inventory     { get; private set; }
        public IPawnEffect      PawnEffects   { get; private set; }
        public TerrainCostMap   MovementCosts { get; private set; }
       
        public event Action OnDefeated;

        private void Awake()
        {
            SpawnPawn(Config);
        }

        private void SpawnPawn(PawnConfig  config)
        {
            if (!config)
            {
                Debug.LogError("Missing Config to draw from");
                return;
            }

            _icon         = config.icon;
            Stats         = new PawnStats(config);
            Inventory     = new TetrisContainer(new Vector2Int(6, 3));
            PawnEffects   = _pawnEffects;
            MovementCosts = config.movementCosts;
            
            if (config.starterWeapon != null)
                Inventory.TryAdd(ItemFactory.Create(config.starterWeapon));
            else
                Debug.LogWarning($"{gameObject.name} has no StarterWeapon assigned in PawnConfig.", this);

            Stats.health.OnDepleted += DespawnPawn;
            //_healthView.SetPawn(Stats.health);
            //_manaView.SetPawn(Stats.mana);
            
            gameObject.SetActive(true);
        }

        private void DespawnPawn()
        {
            Debug.Log($"{gameObject.name} has been defeated!");
            OnDefeated?.Invoke();
            
            gameObject.SetActive(false);
        }

        public void TakeDamage(float damage) => Stats.health.ReduceCurrent(damage);
        
        public void MoveTo(Hex hex)
        {
            HexPosition = hex;
        }
    }

    public interface IPawn : IHexOccupant, ICombatParticipant
    {
        IPawnEffect      PawnEffects   { get; }
        IPawnStats       Stats         { get; }
        ITetrisContainer Inventory     { get; }
        TerrainCostMap   MovementCosts { get; }
    }

    public interface ICombatParticipant
    {
        PawnTeam Team { get; }
        event Action OnDefeated;
        void TakeDamage(float damage);
    }
}