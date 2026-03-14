namespace Code.Data
{
    public static class Const
    {
        public const string ConfigRoot = "Configs/";
        public const int InventoryCellSize = 64;
        public const int InventoryPadding = 8;
        
        #region Chain Colors

        public static readonly UnityEngine.Color WeaponRootColor   = HexColor("#B72C10");
        public static readonly UnityEngine.Color PayloadColor      = HexColor("#9B62C8");
        public static readonly UnityEngine.Color ActivatorColor    = HexColor("#206BB6");
        public static readonly UnityEngine.Color ReactorColor      = HexColor("#67B7E0");
        public static readonly UnityEngine.Color AmplifierColor    = HexColor("#E0AF3E");
        public static readonly UnityEngine.Color ConverterColor    = HexColor("#71675B");

        private static UnityEngine.Color HexColor(string hex)
        {
            UnityEngine.ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
        
        #endregion
    }
}