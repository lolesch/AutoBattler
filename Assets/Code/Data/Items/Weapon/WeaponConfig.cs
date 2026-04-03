using System.Collections.Generic;
using Code.Data.Enums;
using UnityEngine;

namespace Code.Data.Items.Weapon
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = Const.ItemConfig + "Weapon")]
    public sealed class WeaponConfig : ItemConfig
    {
        [field: Header("Weapon Properties")]
        [field: SerializeField] public float BaseDamage   { get; private set; }
        [field: SerializeField] public float AttackSpeed  { get; private set; }
        [field: SerializeField] public float ResourceCost { get; private set; }
        [field: SerializeField] public float ResourceGenOnHit { get; private set; }

        [field: Header("Payload")]
        //TODO: rework conditions!
        [field: SerializeField] public ConditionType        PayloadCondition          { get; private set; }
       [field: SerializeField] public float                PayloadConditionThreshold { get; private set; } = 0.5f;

        public override int MaxConnectors => 2;
        
        // CONTINUE HERE
        public WeaponTags tags;

        //public DeliveryProfile delivery;
        //public PayloadModifier payload;
        //public StatusEffect status;
    }

    [System.Serializable]
    public class DeliveryProfile
    {
        public float range;
        public float radius;
        public int chainCount;

        public float tickRate;
        public float duration;

        public bool requiresLOS;
    }
    [System.Serializable]
    public class PayloadModifier
    {
        public float powerMultiplier = 1f;

        public List<ModifierEffect> effects;
    }
    [System.Serializable]
    public class ModifierEffect
    {
        public ModifierType type;
        public float value;
    }
    public enum ModifierType
    {
        AddPierce,
        AddSplit,
        AddDelay,
        AddRepeat,
        IncreaseAoE,
        ApplyStatus,
        ModifyTargeting,
        CreateTerrain,
        ApplyForce
    }
    
    [CreateAssetMenu(fileName = "StatusEffect", menuName = Const.ConfigRoot + "StatusEffect")]
    public class StatusEffect : ScriptableObject
    {
        public string statusName;

        public int maxStacks;
        public float duration;

        public WeaponTags tags;

        public List<StatusBehavior> behaviors;
    }
    
    [System.Serializable]
    public class StatusBehavior
    {
        public StatusTrigger trigger;
        public StatusEffectType effectType;

        public float value;
    }
    public enum StatusTrigger
    {
        OnTick,
        OnHit,
        OnMove,
        OnExpire
    }
    public enum StatusEffectType
    {
        Damage,
        Slow,
        Root,
        Spread,
        Detonate,
        Amplify
    }
    public class TerrainTile
    {
        public TerrainType type;

        public List<TerrainModifier> modifiers;
    }
    public enum TerrainType
    {
        Normal,
        Burning,
        Frozen,
        Toxic,
        Conductive,
        Obstructed
    }
    [System.Serializable]
    public class TerrainModifier
    {
        public StatusEffect appliedStatus;
        public float intensity;
    }
}