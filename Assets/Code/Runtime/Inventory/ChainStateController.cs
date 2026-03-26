using System.Collections.Generic;
using Code.Runtime.Statistics;

namespace Code.Runtime.Inventory
{
    /// <summary>
    /// Tracks which items transition between unchained (applying pawn stat mods)
    /// and chained (pawn mods removed, chain effect active).
    /// Subscribes to OnContentsChanged and diffs topology each time the inventory changes.
    /// </summary>
    public sealed class ChainStateController
    {
        private readonly ITetrisContainer _inventory;
        private readonly IPawnStats       _stats;
        private HashSet<ITetrisItem>      _chained = new();

        public ChainStateController(ITetrisContainer inventory, IPawnStats stats)
        {
            _inventory = inventory;
            _stats     = stats;
            _inventory.OnContentsChanged += _ => Refresh();
        }

        private void Refresh()
        {
            var topology  = ChainResolver.ResolveTopology(_inventory);
            var nowChained = new HashSet<ITetrisItem>();

            foreach (var chain in topology.Chains)
            {
                nowChained.Add(chain.Root);
                foreach (var mod in chain.Modifiers)
                    nowChained.Add(mod);
            }

            // Newly chained — remove pawn mods
            foreach (var item in nowChained)
                if (!_chained.Contains(item) && item is IEquippable eq)
                    eq.OnUnequipped(_stats);

            // Newly unchained — re-apply pawn mods
            foreach (var item in _chained)
                if (!nowChained.Contains(item) && item is IEquippable eq)
                    eq.OnEquipped(_stats);

            _chained = nowChained;
        }
    }
}
