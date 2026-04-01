using System;
using Code.Data.Enums;
using Code.Data.Pawns;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public sealed class PawnStats : IPawnStats
    {
        [field: SerializeField, ReadOnly, AllowNesting] public Resource health      { get; private set; }
        [field: FormerlySerializedAs("<Mana>k__BackingField")] [field: SerializeField, ReadOnly, AllowNesting] public Resource mana        { get; private set; }

        public PawnStats(PawnConfig config)
        {
            health      = new Resource(PawnStatType.MaxLife,      config.baseHealth);
            mana        = new Resource(PawnStatType.MaxMana,      config.baseMana);
        }

        private Stat GetStat(PawnStatType type) => type switch
        {
            PawnStatType.MaxLife     => health,
            PawnStatType.MaxMana     => mana,
            _                    => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public void ApplyMod(PawnStatModifier mod)  => GetStat(mod.PawnStat)?.AddModifier(mod.Modifier);
        public void RemoveMod(PawnStatModifier mod) => GetStat(mod.PawnStat)?.TryRemoveModifier(mod.Modifier);
    }

    public interface IPawnStats
    {
        Resource health      { get; }
        Resource mana        { get; }

        void ApplyMod(PawnStatModifier mod);
        void RemoveMod(PawnStatModifier mod);
    }
}