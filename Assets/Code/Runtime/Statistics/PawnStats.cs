using System;
using Code.Data.Enums;
using Code.Data.Pawns;
using NaughtyAttributes;
using UnityEngine;

namespace Code.Runtime.Statistics
{
    [Serializable]
    public sealed class PawnStats : IPawnStats
    {
        [field: SerializeField, ReadOnly, AllowNesting] public Resource health      { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Resource Mana        { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Stat     damage      { get; private set; }
        [field: SerializeField, ReadOnly, AllowNesting] public Stat     attackSpeed { get; private set; }

        public PawnStats(PawnConfig config)
        {
            health      = new Resource(StatType.MaxLife,      config.baseHealth);
            Mana        = new Resource(StatType.MaxMana,      config.baseMana);
            damage      = new Stat(StatType.Damage,           config.baseDamage);
            attackSpeed = new Stat(StatType.AttackSpeed,      config.baseAttackSpeed);
        }

        private Stat GetStat(StatType type) => type switch
        {
            StatType.Damage      => damage,
            StatType.MaxLife     => health,
            StatType.AttackSpeed => attackSpeed,
            StatType.MaxMana     => Mana,
            _                    => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public void ApplyMod(PawnStatModifier mod)  => GetStat(mod.stat)?.AddModifier(mod.modifier);
        public void RemoveMod(PawnStatModifier mod) => GetStat(mod.stat)?.TryRemoveModifier(mod.modifier);
    }

    public interface IPawnStats
    {
        Resource health      { get; }
        Resource Mana        { get; }
        Stat     damage      { get; }
        Stat     attackSpeed { get; }

        void ApplyMod(PawnStatModifier mod);
        void RemoveMod(PawnStatModifier mod);
    }
}