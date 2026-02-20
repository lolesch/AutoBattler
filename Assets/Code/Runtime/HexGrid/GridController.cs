using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.HexGrid
{
    [Serializable]
    public sealed class GridController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int gridRange = 5;
        [SerializeField] private float hexSpacing = 1f;
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask mask;
        [SerializeField, ReadOnly] private Hex hoveredHex = Hex.Invalid;
        [SerializeField, ReadOnly] private Hex selectedHex = Hex.Invalid;
        public static float Circumradius { get; private set; }
        public static float HexWidth { get; private set; }

        public static event Action<Hex> OnHexHovered;
        public static event Action<Hex> OnHexSelected;
        
        void OnValidate()
        {
            HexWidth = hexSpacing;
            Circumradius = hexSpacing / Mathf.Sqrt( 3 );
        }

        private void Update()
        {
            if( !Physics.Raycast( cam.ScreenPointToRay( Input.mousePosition ), out RaycastHit raycastHit,
                   float.MaxValue, mask ) )
                return;
            
            var currentHover = raycastHit.point.WorldToHex( HexWidth, Circumradius );
            
            if (hoveredHex != currentHover)
            {
                OnHexHovered?.Invoke(Hex.Invalid);

                hoveredHex = currentHover;
                OnHexHovered?.Invoke(hoveredHex);
            }

            if (Input.GetMouseButtonDown(0))
            {
                selectedHex = hoveredHex;
                OnHexSelected?.Invoke(selectedHex);

                Debug.Log($"selected: {selectedHex}");
            }
        }

        private void OnDrawGizmos()
        {
            var gridPositions = Hex.zero.HexRange( gridRange, false );
            foreach (var hex in gridPositions)
                DrawHexagonOnXZPlane( hex, Color.white );
            
            if (hoveredHex != Hex.Invalid)
                DrawHexagonOnXZPlane( hoveredHex, Color.orange );
            
            if (selectedHex != Hex.Invalid)
                DrawHexagonOnXZPlane( selectedHex, Color.cyan );
        }

        private static void DrawHexagonOnXZPlane(Hex hex, Color color )
        {
            var center = hex.ToWorldPos( HexWidth, Circumradius );
            
            var corners = new Vector3[6];
            for (var i = 0; i < 6; i++)
            {
                var angle_deg = 60 * i - 30;
                var angle_rad = Mathf.PI / 180 * angle_deg;
                
                corners[i] = new Vector3(
                    center.x + Circumradius * Mathf.Cos(angle_rad),
                    center.y,
                    center.z + Circumradius * Mathf.Sin(angle_rad)
                );
            }

            Gizmos.color = color;
            for (var i = 0; i < 6; i++)
            {
                var next = (i + 1) % 6;
                Gizmos.DrawLine(corners[i], corners[next]);
            }
            Gizmos.color = Color.white;
        }
    }
}

