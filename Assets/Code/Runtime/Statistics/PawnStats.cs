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
        [field: SerializeField, ReadOnly, AllowNesting] public Resource mana        { get; private set; }
        
        [field: SerializeField, ReadOnly, AllowNesting] public Stat healthRegen        { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Stat manaRegen        { get; private set; }

        public PawnStats(PawnConfig config)
        {
            health      = new Resource(PawnStatType.LifeMax, config.baseHealth);
            healthRegen = new Stat(PawnStatType.LifeRegen,   config.baseHealthRegen);
            mana        = new Resource(PawnStatType.ManaMax, config.baseMana);
            manaRegen   = new Stat(PawnStatType.ManaRegen,   config.baseManaRegen);
        }

        private Stat GetStat(PawnStatType type) => type switch
        {
            PawnStatType.LifeMax   => health,
            PawnStatType.ManaMax   => mana,
            PawnStatType.LifeRegen => healthRegen,
            PawnStatType.ManaRegen => manaRegen,
            PawnStatType.None or _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
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