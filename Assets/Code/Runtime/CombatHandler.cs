using Code.Data.Items.Amplifier;
using Code.Data.Items.Weapon;
using Code.Runtime.Container.Items;
using Code.Runtime.GUI.Inventory;
using Submodules.Utility.Tools.Timer;
using UnityEngine;

namespace Code.Runtime
{
    /// <summary>
    /// Thin combat coordinator.
    /// Responsible for: wiring pawn targets, starting/stopping combat, and owning target-finding logic.
    /// All per-pawn timer and chain logic lives in PawnCombatController.
    /// All hex selection and UI logic lives in HexSelectionHandler.
    /// </summary>
    public sealed class CombatHandler : MonoBehaviour
    {
        [SerializeField] private Pawn player;
        [SerializeField] private Pawn enemy;

        [Header("Item Testing")]
        [SerializeField, Range(1, 18)] private int          amountOfItems  = 1;
        [SerializeField]               private WeaponConfig  weaponConfig;
        [SerializeField]               private AmplifierConfig amplifierConfig;
        [SerializeField]               private RotationType  rotation;
        [SerializeField]               private InventoryView inventoryView;

        private void Start()
        {
            player.CombatController.SetTarget(enemy);
            enemy.CombatController.SetTarget(player);

            // Enemy has no weapon chain yet — drive its attacks directly from PawnStats
            // TODO: remove once enemy has a weapon item and uses CombatController fully
            var enemyFallbackTimer = new Timer(enemy.Stats.attackSpeed, true);
            enemyFallbackTimer.OnRewind += () => player.TakeDamage(enemy.Stats.damage);
            //enemyFallbackTimer.Start();
        }

        [ContextMenu("StartCombat")]
        private void StartCombat()
        {
            player.CombatController.StartCombat();
            enemy.CombatController.StartCombat();
        }

        // ── Item testing ──────────────────────────────────────────────────

        [ContextMenu("AddWeaponToPlayer")]
        private void AddWeaponToPlayer() => AddWeapon(player);
        
        [ContextMenu("AddWeaponToEnemy")]
        private void AddWeaponToEnemy() => AddWeapon(enemy);
        
        private void AddWeapon( IPawn pawn)
        {
            if (weaponConfig == null)
            {
                Debug.LogWarning("No WeaponConfig assigned.");
                return;
            }

            for (var i = 0; i < amountOfItems; i++)
                pawn.EquipItem(new WeaponItem(weaponConfig, rotation));

            inventoryView.RefreshView(pawn);
        }

        [ContextMenu("AddAmplifierToPlayer")]
        private void AddAmplifierToPlayer() => AddAmplifier(player);
        
        [ContextMenu("AddAmplifierToEnemy")]
        private void AddAmplifierToEnemy() => AddAmplifier(enemy);

        private void AddAmplifier(IPawn pawn)
        {
            if (amplifierConfig == null)
            {
                Debug.LogWarning("No AmplifierConfig assigned.");
                return;
            }

            for (var i = 0; i < amountOfItems; i++)
                pawn.EquipItem(new AmplifierItem(amplifierConfig, rotation));

            inventoryView.RefreshView(pawn);
        }
    }
}