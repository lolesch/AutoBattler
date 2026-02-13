using Submodules.Utility.Attributes;
using UnityEngine;

namespace Code.Data.SO
{
    [CreateAssetMenu(fileName = "PawnConfig", menuName = Const.ConfigRoot + "Pawns")]
    public sealed class PawnConfig : ScriptableObject
    {
        [PreviewIcon] public Sprite icon;
        [Min(1)] public uint baseHealth = 100;
        [Min(1)] public uint baseDamage = 10;
        [Min(1)] public uint baseAttackSpeed = 1;
    }
}
