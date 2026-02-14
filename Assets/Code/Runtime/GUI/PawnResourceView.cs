using Code.Runtime.Statistics;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.GUI
{
    //[RequireComponent(typeof( Image ))]
    public sealed class PawnResourceView : MonoBehaviour
    {
        [SerializeField] private Resource resource;
        [SerializeField] private Image healthbar;
        
        public void SetPawn( Resource res )
        {
            if( res == null )
                return;
            
            var previous = resource;
            if( previous != null ) 
                previous.OnCurrentChanged -= UpdateView;
            
            resource = res;
            resource.OnCurrentChanged += UpdateView;
        }

        private void UpdateView( float prev, float curr, float max ) => healthbar.fillAmount = resource.Percentage;
    }
}