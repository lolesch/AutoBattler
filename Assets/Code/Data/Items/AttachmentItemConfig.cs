using System;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items
{
    [Serializable]
    public abstract class AttachmentItemConfig : ItemConfig
    {
        [field: Header("Unchained")] 
        public PawnStatModConfig pawnStatMod;
    }

    [Serializable]
    public abstract class StatModConfig<T> where T : Enum
    {
        public T stat;
        public float value;
        public ModifierType type;
    }
    
    [Serializable]
    public class PawnStatModConfig : StatModConfig<PawnStatType> {}
    [Serializable]
    public class WeaponUsageStatModConfig : StatModConfig<UsageStatType> {}
    [Serializable]
    public class WeaponAttackStatModConfig : StatModConfig<AttackStatType> {}
}