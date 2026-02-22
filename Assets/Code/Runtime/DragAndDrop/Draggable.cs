using UnityEngine;
using UnityEngine.Tilemaps;

namespace Code.Runtime.DragAndDrop
{
    public sealed class Draggable : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pickupClip, dropClip;
        [SerializeField] private Transform pawn;
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap tilemap;
        
        private bool isDragging;

        private Vector2 offset;
        private Vector2 previousPos;

        private void Awake()
        {
            previousPos = pawn.position;
        }

        private void OnMouseDrag()
        {
            if(!isDragging)
                return;
            
            var mousePos = GetMousePos() - offset;
            pawn.position = mousePos;
        }

        private void OnMouseDown()
        {
            isDragging = true;
            audioSource.PlayOneShot(pickupClip);
            
            offset = GetMousePos() - (Vector2)pawn.position;
        }
        
        private void OnMouseUp()
        {
            isDragging = false;
            audioSource.PlayOneShot(dropClip);

            var cell = grid.WorldToCell( pawn.position );

            if( tilemap.HasTile( cell ) )
            {
                pawn.position = grid.CellToWorld( cell );
                previousPos = pawn.position;
            }
            else
                pawn.position = previousPos;
        }

        private Vector2 GetMousePos() => cam.ScreenToWorldPoint( Input.mousePosition );
    }
}
