using NaughtyAttributes;
using Submodules.Utility.Tools;
using TMPro;
using UnityEngine;

namespace Code.Runtime.GUI
{
    [RequireComponent( typeof( TextMeshProUGUI ) )]
    public sealed class BundleVersionView : MonoBehaviour
    {
        [SerializeField, ReadOnly] private TextMeshProUGUI versionText;

        [ContextMenu("Refresh")]
        private void Start() => RefreshVersionText( BundleVersionSetter.GetVersion() );

        private void RefreshVersionText(string versionNumber)
        {
            if (!versionText)
                versionText = GetComponent<TextMeshProUGUI>();

            if (versionText)
                versionText.text = $"v{versionNumber}";
        }
    }
}