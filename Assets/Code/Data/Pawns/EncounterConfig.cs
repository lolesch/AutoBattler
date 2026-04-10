using System;
using System.Collections.Generic;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Data.Pawns
{
    [CreateAssetMenu(fileName = "EncounterConfig", menuName = Const.ConfigRoot + "Encounters")]
    public sealed class EncounterConfig : ScriptableObject
    {
        public List<SpawnData> enemies;
        public List<SpawnData> players;

        [Serializable]
        public struct SpawnData
        {
            public PawnConfig config;
            public Hex startHex;
        }
    }
}