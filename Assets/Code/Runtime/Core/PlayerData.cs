using System.Collections.Generic;
using Code.Data.Pawns;
using Code.Runtime.Modules.Inventory;
using Code.Runtime.Pawns;
using UnityEngine;

namespace Code.Runtime.Core
{
    public sealed class PlayerData : IPlayerData
    {
        public ITetrisContainer Stash { get; }
        public EncounterConfig currentEncounter { get; set; }
        public List<PawnConfig> OwnedPawns { get; }
        public List<IPawn> DeployedPawns { get; }
        
        // all player pawns, on the battlefield or in the bank, if that is a feature
        
        //store the active combat
        /* the current map,
         * all terrain changes
         * the current pawns on the map with their positions
         * ...
         */

        public PlayerData(Vector2Int stashSize, EncounterConfig currentEncounter)
        {
            Stash = new TetrisContainer(stashSize);
            this.currentEncounter = currentEncounter;
        }
    }

    public interface IPlayerData
    {
        ITetrisContainer Stash { get; }
        EncounterConfig currentEncounter { get; set; }
        List<PawnConfig> OwnedPawns { get; }
        List<IPawn> DeployedPawns   { get; }
    }
}