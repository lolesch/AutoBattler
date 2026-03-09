using Code.Runtime.Inventory;
using UnityEngine;

namespace Code.Runtime.GameLoop
{
    /// <summary>
    /// Owns all player-persistent state across phases.
    /// Stash is a TetrisContainer with no owning pawn — items placed here
    /// do not apply stat modifiers until dragged into a unit's inventory.
    /// </summary>
    public sealed class PlayerData : IPlayerData
    {
        public ITetrisContainer Stash { get; }

        public PlayerData(Vector2Int stashSize)
        {
            // Passing null stats — TetrisContainer skips OnEquipped/OnUnequipped when stats is null.
            // Items in the stash are inert until moved into a pawn inventory.
            Stash = new TetrisContainer(stashSize, null);
        }
    }

    public interface IPlayerData
    {
        ITetrisContainer Stash { get; }
    }
}