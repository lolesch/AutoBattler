using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Code.Runtime.UI.Roster
{
    public sealed class RosterView : MonoBehaviour
    {
        [SerializeField] private PawnCardView             cardPrefab;
        //[SerializeField] private PawnDragController _dragController;

        [SerializeField, ReadOnly, AllowNesting] private PawnCardView[] slots;

        public IReadOnlyList<IPawnCardView> Slots => slots.ToList();
    }
}