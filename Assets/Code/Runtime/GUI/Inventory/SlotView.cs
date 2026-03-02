using Code.Runtime.Container.Items;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.GUI.Inventory
{
    public sealed class SlotView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private GridLayoutGroup _grid;

        private float CellSize => _grid.cellSize.x;
        private float Spacing => _grid.spacing.x;

        private void Awake() => _grid ??= GetComponentInParent<GridLayoutGroup>();


        private void OnValidate()
        {
            icon.rectTransform.anchorMin = new Vector2(0, 1);
            icon.rectTransform.anchorMax = new Vector2(0, 1);
            icon.rectTransform.anchoredPosition = Vector2.zero;
        }

        public void RefreshView( TetrisItem item )
        {
            var hasItem = item != null;
    
            if( hasItem )
            {
                var origin = item.GetShapeOrigin();
                var dims = item.GetDimensions();
                var isRotated90Or270 = item.rotation is RotationType.CW90 or RotationType.CW270;
                var w = isRotated90Or270 ? dims.y : dims.x;
                var h = isRotated90Or270 ? dims.x : dims.y;

                icon.sprite = item.Icon;
                icon.rectTransform.localEulerAngles = new Vector3( 0f, 0f, (int)item.rotation * 90f );
                icon.rectTransform.pivot = CalculatePivot( origin, dims, item.rotation );
                icon.rectTransform.sizeDelta = new Vector2(
                    w * CellSize + ( w - 1 ) * Spacing,
                    h * CellSize + ( h - 1 ) * Spacing
                );
            }
            else
            {
                icon.rectTransform.sizeDelta = Vector2.one * CellSize;
                icon.rectTransform.anchoredPosition = Vector2.zero;
            }

            icon.color = hasItem ? Color.white : Color.clear;
        }

        private static Vector2 CalculatePivot( Vector2Int origin, Vector2Int dims, RotationType rotation ) =>
            rotation switch
            {
                RotationType.None   => new Vector2( origin.x / (float)dims.x,        1f - origin.y / (float)dims.y ),
                RotationType.CW90   => new Vector2( 1f - origin.y / (float)dims.y,   1f - origin.x / (float)dims.x ),
                RotationType.CW180  => new Vector2( 1f - origin.x / (float)dims.x,   origin.y / (float)dims.y ),
                RotationType.CW270  => new Vector2( origin.y / (float)dims.y,        origin.x / (float)dims.x ),
                _                   => new Vector2( 0.5f, 0.5f ),
            };
    }
}
