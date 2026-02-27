using System;
using UnityEditor;
using UnityEngine;

namespace Code.Runtime.Grids.HexGridInspector.Editor
{
    public class EditorWindowIntField : EditorWindow
    {
        private const string controlName = "IntField";
        private static bool controlFocused;

        public delegate void OnApply(int newSize);

        private static int newFieldValue;
        private static string fieldLabel;
        private static OnApply onApply;

        public static void ShowWindow(int fieldValue, OnApply onApplyCallback, string label)
        {
            newFieldValue = fieldValue;
            onApply = onApplyCallback;
            fieldLabel = label;

            controlFocused = false;

            // Get existing open window or if none, make a new one
            var window = GetWindow<EditorWindowIntField>();
            window.maxSize = new Vector2(250, 100);

            window.ShowPopup();
        }

        private void OnGUI()
        {
            UnityEngine.GUI.SetNextControlName(controlName);
            newFieldValue = EditorGUILayout.IntField(fieldLabel, Math.Abs(newFieldValue));

            if (!controlFocused)
            {
                EditorGUI.FocusTextInControl(controlName);
                controlFocused = true;
            }

            var wrongFieldValue = newFieldValue < 0;

            if (wrongFieldValue)
                EditorGUILayout.HelpBox($"Wrong {fieldLabel}.", MessageType.Error);

            UnityEngine.GUI.enabled = !wrongFieldValue;

            if (GUILayout.Button("Apply"))
                Apply();

            UnityEngine.GUI.enabled = true;

            // We're doing this in OnGUI() since the Update() function doesn't seem to get called when we show the window with ShowModalUtility().
            var ev = Event.current;
            if (ev.type is EventType.KeyDown or EventType.KeyUp)
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
