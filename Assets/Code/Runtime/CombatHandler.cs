using System.Collections.Generic;
using Code.Data.Items;
using Code.Data.Items.Amplifier;
using Code.Data.Items.Weapon;
using Code.Runtime.Container.Items;
using Code.Runtime.Container.Items.Chain;
using Code.Runtime.GUI.Inventory;
using NaughtyAttributes;
using Submodules.Utility.Extensions;
using Submodules.Utility.Tools.Timer;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Runtime
{
    public class CombatHandler : MonoBehaviour
    {
        [SerializeField] private Pawn player;
        [SerializeField] private Pawn enemy;
        [SerializeField] private Timer enemyTimer;
        [SerializeField] private InventoryView inventoryView;

        [Header("Item Testing")] 
        [SerializeField, Range(1,18)] private int amountOfItems = 1;
        [Tooltip("Assign a WeaponConfig to add a WeaponItem to the player.")]
        [SerializeField] private WeaponConfig weaponConfig;
        [Tooltip("Assign an AmplifierConfig to chain an amplifier after the weapon.")]
        [SerializeField] private AmplifierConfig amplifierConfig;
        [SerializeField] private RotationType rotation;

        [Header("Hex Map")]
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap levelMap;
        [SerializeField] private Tilemap pawnEffectMap;
        [SerializeField] private TileBase effectTile;

        // One timer per active weapon chain — rebuilt on every inventory change
        private readonly List<Timer> _playerWeaponTimers = new();

        private Plane      _plane        = new Plane(Vector3.back, 0f);
        private Vector3Int _selectedCell;
        private Camera     _cam;
        private Pawn       _selectedPawn;

        private void Awake()
        {
            if (!_cam)
                _cam = Camera.main;
        }

        private void Start()
        {
            player.inventory.OnContentsChanged += _ => RebuildPlayerChains();

            enemyTimer = new Timer(enemy.stats.attackSpeed, true);
            enemyTimer.OnRewind += () => player.TakeDamage(enemy.stats.damage);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
                player.pawnEffects.Rotate(false);
            if (Input.GetKeyDown(KeyCode.E))
                player.pawnEffects.Rotate(true);

            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (!_plane.Raycast(ray, out var distance))
                return;

            var cell = grid.WorldToCell(ray.GetPoint(distance));
            if (!levelMap.HasTile(cell) || _selectedCell == cell)
                return;

            _selectedCell = cell;
            CheckHexForUnit();
        }

        [ContextMenu("StartCombat")]
        private void StartCombat()
        {
            foreach (var timer in _playerWeaponTimers)
                timer.Start();
            enemyTimer.Start();
        }

        /// <summary>
        /// Adds a WeaponItem to the player inventory at the first available slot.
        /// Chain re-resolves automatically via OnContentsChanged.
        /// </summary>
        [ContextMenu("AddWeaponToPlayer")]
        private void AddWeaponToPlayer()
        {
            if (weaponConfig == null)
            {
                Debug.LogWarning("No WeaponConfig assigned.");
                return;
            }

            var weapon = new WeaponItem(weaponConfig, rotation);
            for (int i = 0; i < amountOfItems; i++)
                player.EquipItem(weapon);
            inventoryView.RefreshView(player);
        }

        /// <summary>
        /// Adds an AmplifierItem to the player inventory.
        /// For this to connect, its input slot must be adjacent to the weapon's output slot.
        /// </summary>
        [ContextMenu("AddAmplifierToPlayer")]
        private void AddAmplifierToPlayer()
        {
            if (amplifierConfig == null)
            {
                Debug.LogWarning("No AmplifierConfig assigned.");
                return;
            }

            var amp = new AmplifierItem(amplifierConfig, rotation);
            for (int i = 0; i < amountOfItems; i++)
                player.EquipItem(amp);
            inventoryView.RefreshView(player);
        }

        private void RebuildPlayerChains()
        {
            foreach (var timer in _playerWeaponTimers)
                timer.Stop();
            _playerWeaponTimers.Clear();

            var chains = ChainResolver.Resolve(player.inventory);

            if (chains.Count == 0)
            {
                Debug.Log("[Chain] No weapons found — player is not attacking.");
                return;
            }

            foreach (var chain in chains)
            {
                var stats = chain.Resolve();

                Debug.Log($"[Chain:{chain.Weapon.Name}] " +
                          $"Damage: {stats.Damage} | " +
                          $"AttackSpeed: {stats.AttackSpeed} | " +
                          $"ResourceCost: {stats.ResourceCost}");

                var capturedStats = stats;
                var timer = new Timer(capturedStats.AttackSpeed, true);
                timer.OnRewind += () => enemy.TakeDamage(capturedStats.Damage);
                timer.Start();
                _playerWeaponTimers.Add(timer);
            }
        }

        private void CheckHexForUnit()
        {
            pawnEffectMap.ClearAllTiles();

            foreach (var pawn in new[] { player, enemy })
            {
                var pawnCell = grid.WorldToCell(pawn.transform.position);

                if (pawnCell != _selectedCell)
                    continue;

                if (_selectedPawn != pawn)
                {
                    _selectedPawn = pawn;
                    inventoryView.RefreshView(pawn);
                }

                foreach (var hex in pawn.pawnEffects.GetHexes())
                {
                    var cell = pawnCell.CellToHex().Add(hex).ToCell();
                    pawnEffectMap.SetTile(cell, effectTile);
                }
            }
        }
    }
}