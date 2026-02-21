using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Runtime.DragAndDrop
{
    public sealed class Draggable : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pickupClip, dropClip;
        [SerializeField] private Transform pickupTransform;
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap tilemap;
        
        private bool isDragging;

        private Vector2 offset;
        private Vector2 originalPosition;

        private void Awake()
        {
            originalPosition = pickupTransform.position;
        }

        private void OnMouseDrag()
        {
            if(!isDragging)
                return;
            
            var mousePos = GetMousePos() - offset;
            pickupTransform.position = mousePos;
        }

        private Vector2 GetMousePos() => cam.ScreenToWorldPoint( Input.mousePosition );

        private void OnMouseDown()
        {
            isDragging = true;
            audioSource.PlayOneShot(pickupClip);
            
            offset = GetMousePos() - (Vector2)pickupTransform.position;
        }
        
        private void OnMouseUp()
        {
            isDragging = false;
            audioSource.PlayOneShot(dropClip);

            var cell = grid.WorldToCell( pickupTransform.position );

            if( tilemap.HasTile( cell ) )
            {
                pickupTransform.position = grid.CellToWorld( cell );
                originalPosition = pickupTransform.position;
            }
            else
                pickupTransform.position = originalPosition;
        }
    }
}
