using UnityEditor;
using UnityEngine;

namespace Code.Runtime.Grids.RectGridInspector.Editor
{
    public class EditorWindowVector2IntField : EditorWindow
    {
        private const string controlName = "Vector2IntField";
        private static bool controlFocused;

        public delegate void OnApply(Vector2Int newSize);

        private static Vector2Int newFieldValue;
        private static string fieldLabel;
        private static OnApply onApply;


        public static void ShowWindow(string title, Vector2Int fieldValue, OnApply onApplyCallback, string label)
        {
            newFieldValue = fieldValue;
            onApply = onApplyCallback;
            fieldLabel = label;
        
            controlFocused = false;

            // Get existing open window or if none, make a new one
            var window = GetWindow<EditorWindowVector2IntField>();
            window.titleContent = new GUIContent(title);
            window.maxSize = new Vector2(250, 100);

            window.ShowPopup();
        }

        void OnGUI()
        {
            UnityEngine.GUI.SetNextControlName(controlName);
            newFieldValue = EditorGUILayout.Vector2IntField(fieldLabel, newFieldValue);

            if (!controlFocused)
            {
                EditorGUI.FocusTextInControl(controlName);
                controlFocused = true;
            }

            var wrongFieldValue = (newFieldValue.x <= 0 || newFieldValue.y <= 0);

            if (wrongFieldValue)
                EditorGUILayout.HelpBox($"Wrong {fieldLabel}.", MessageType.Error);
        
            UnityEngine.GUI.enabled = !wrongFieldValue;

            if (GUILayout.Button("Apply"))
                Apply();
        
            UnityEngine.GUI.enabled = true;

            // We're doing this in OnGUI() since the Update() function doesn't seem to get called when we show the window with ShowModalUtility().
            var ev = Event.current;
            if (ev.type == EventType.KeyDown || ev.type == EventType.KeyUp)
            {
                switch (ev.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        Apply();
                        break;
                    case KeyCode.Escape:
                        Close();
                        break;
                }
            }
        }

        private void Apply()
        {
            onApply?.Invoke(newFieldValue);
            Close();
        }
    }
}