using Code.Data.Items.Weapon;
using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Data.Pawns
{
    [CreateAssetMenu(fileName = "PawnConfig", menuName = Const.ConfigRoot + "Pawns")]
    public sealed class PawnConfig : ScriptableObject
    {
        [PreviewIcon] public Sprite icon;
        [Min(1)] public uint baseHealth = 100;
        [Min(1)] public uint baseMana = 60;
        public WeaponConfig StarterWeapon;
    }
}
