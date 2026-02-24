using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.GUI.Inventory
{
    public sealed class SlotView : MonoBehaviour
    {
        [SerializeField] private Image icon;
    
        public void RefreshView( Item item )
        {
            var hasItem = item != null;
            if( hasItem )
                icon.sprite = item.Icon;
        
            icon.color = hasItem? Color.white : Color.clear;
        }
    }
}
