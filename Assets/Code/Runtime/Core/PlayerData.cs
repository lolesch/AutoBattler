using Code.Runtime.Modules.Inventory;
using UnityEngine;

namespace Code.Runtime.Core
{
    public sealed class PlayerData : IPlayerData
    {
        public ITetrisContainer Stash { get; }

        public PlayerData(Vector2Int stashSize)
        {
            Stash = new TetrisContainer(stashSize);
        }
    }

    public interface IPlayerData
    {
        ITetrisContainer Stash { get; }
    }
}