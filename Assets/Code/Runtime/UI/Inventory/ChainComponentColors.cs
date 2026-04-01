using Code.Data;
using Code.Runtime.Inventory;
using UnityEngine;

namespace Code.Runtime.UI.Inventory
{
    public static class ChainComponentColors
    {
        public static Color GetColor(ITetrisItem item, bool isRoot) => item switch
        {
            IWeaponItem    when isRoot => Const.WeaponRootColor,
            IWeaponItem                => Const.PayloadColor,
            IAmplifierItem             => Const.AmplifierColor,
            IConverterItem             => Const.ConverterColor,
            IShifterItem             => Const.ActivatorColor,
            IReactorItem               => Const.ReactorColor,
            _                          => Color.white,
        };
    }
}