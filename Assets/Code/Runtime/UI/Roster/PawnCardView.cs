using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.UI.Roster
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public sealed class PawnCardView : MonoBehaviour,IPawnCardView
    {
        
    }

    public interface IPawnCardView
    {
        
    }
}