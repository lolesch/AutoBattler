using Code.Data.Items;
using UnityEngine;

namespace Code.Data.Items.Weapon
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Configs/Items/Weapon")]
    public sealed class WeaponConfig : ItemConfig
    {
        [Header("Weapon Properties")]
        [field: SerializeField] public float BaseDamage   { get; private set; }
        [field: SerializeField] public float AttackSpeed  { get; private set; }
        [field: SerializeField] public float ResourceCost { get; private set; }

        protected override int MaxConnectors => 2;
    }
}