using System;
using System.Collections.Generic;
using Code.Runtime.HexGrid.HexGridInspector.Runtime;
using NaughtyAttributes;
using Submodules.Utility.Extensions;
using UnityEngine;

namespace Code.Runtime.HexGrid
{
    [Serializable]
    public class GridCreator : MonoBehaviour
    {
        [SerializeField] private HexGridBool MapShape = new(3);

        [SerializeField] private float hexSpacing = 1f;
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask mask;

        public static event Action<Hex> OnHexHovered;
        public static event Action<Hex> OnHexSelected;

        [SerializeField, ReadOnly] private Hex hoveredHex = Hex.Invalid;
        [SerializeField, ReadOnly] private Hex selectedHex = Hex.Invalid;

        private void Awake()
        {
            if (!cam)
                cam = Camera.main;
        }

        private void Update()
        {
            if( !Physics.Raycast( cam.ScreenPointToRay( Input.mousePosition ), out RaycastHit raycastHit,
                   float.MaxValue, mask ) )
                return;
            
            var circumradius = hexSpacing / Mathf.Sqrt( 3 );
            
            var currentHover = raycastHit.point.WorldToHex( hexSpacing, circumradius );
            //var hexAsWorldPos = currentHover.ToWorldPos( hexSpacing, circumradius );
            
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
            var gridPositions = MapShape.GetHexes();
            foreach (var hex in gridPositions)
            {
                Gizmos.color = Color.white;
                DrawHexagonOnXZPlane( hex );
            }
            
            if (hoveredHex != Hex.Invalid)
            {
                Gizmos.color = Color.orange;
                DrawHexagonOnXZPlane( hoveredHex );
            }
            
            if (selectedHex != Hex.Invalid)
            {
                Gizmos.color = Color.cyan;
                DrawHexagonOnXZPlane( selectedHex );
            }
        }

        private void DrawHexagonOnXZPlane(Hex hex)
        {
            var circumradius = hexSpacing / Mathf.Sqrt( 3 );
            var center = hex.ToWorldPos( hexSpacing, circumradius );
            
            var corners = new Vector3[6];
            for (var i = 0; i < 6; i++)
            {
                var angle_deg = 60 * i - 30;
                var angle_rad = Mathf.PI / 180 * angle_deg;
                
                corners[i] = new Vector3(
                    center.x + circumradius * Mathf.Cos(angle_rad),
                    center.y,
                    center.z + circumradius * Mathf.Sin(angle_rad)
                );
            }

            for (var i = 0; i < 6; i++)
            {
                var next = (i + 1) % 6;
                Gizmos.DrawLine(corners[i], corners[next]);
            }
        }

        private ReadOnlySpan<Vector3> GetHexCorners(Vector2 center, float size)
        {
            var span = new List<Vector3>();

            for (var i = 0; i < 6; i++)
            {
                var angle_deg = 60 * i - 30;
                var angle_rad = Mathf.PI / 180 * angle_deg;
                var angle_deg2 = 60 * (i + 1) - 30;
                var angle_rad2 = Mathf.PI / 180 * angle_deg2;

                span.Add(new Vector2(center.x + size * Mathf.Cos(angle_rad), center.y + size * Mathf.Sin(angle_rad)));
                span.Add(new Vector2(center.x + size * Mathf.Cos(angle_rad2), center.y + size * Mathf.Sin(angle_rad2)));
            }

            return new ReadOnlySpan<Vector3>(span.ToArray());
        }
    }
}

