using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Code.Runtime.Grids.HexGridInspector.Editor
{
    // based on: https://github.com/Eldoir/Array2DEditor
    public abstract class HexGridDrawer : PropertyDrawer
    {
        //private const string s_XIconString = "iVBORw0KGgoAAAANSUhEUgAAAA8AAAAPCAYAAAA71pVKAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuNWWFMmUAAABoSURBVDhPnY3BDcAgDAOZhS14dP1O0x2C/LBEgiNSHvfwyZabmV0jZRUpq2zi6f0DJwdcQOEdwwDLypF0zHLMa9+NQRxkQ+ACOT2STVw/q8eY1346ZlE54sYAhVhSDrjwFymrSFnD2gTZpls2OvFUHAAAAABJRU5ErkJggg==";

        private static float LineHeight => EditorGUIUtility.singleLineHeight;

        private const float firstLineMargin = 5f;
        private const float lastLineMargin = 2f;

        private static readonly Vector2 cellSpacing = new(5f, 2f);

        private SerializedProperty thisProperty;
        private SerializedProperty hexRadiusProperty;
        private SerializedProperty cellSizeProperty;
        private SerializedProperty rowsProperty;

        private int Diameter => hexRadiusProperty.intValue * 2 + 1;

        #region SerializedProperty getters

        private void GetHexRadiusProperty(SerializedProperty property) =>
            TryFindPropertyRelative(property, "radius", out hexRadiusProperty);

        private void GetCellSizeProperty(SerializedProperty property) =>
            TryFindPropertyRelative(property, "cellSize", out cellSizeProperty);

        private void GetCellsProperty(SerializedProperty property) =>
            TryFindPropertyRelative(property, "rows", out rowsProperty);

        #endregion

        #region Texts

        private static class Texts
        {
            public static readonly GUIContent reset = new("Reset");
            public static readonly GUIContent resetWithValue = new("Reset with value");
            public static readonly GUIContent changeGridSize = new("Change Hex Radius");
            public static readonly GUIContent changeCellSize = new("Change Cell Size");

            public const string gridSizeLabel = "Hex Radius";
            public const string cellSizeLabel = "Cell Size";
        }

        #endregion

        #region Abstract and virtual methods

        protected virtual Vector2Int GetDefaultCellSizeValue() => new(32, 16);

        protected abstract object GetDefaultCellValue();
        protected abstract object GetFallbackCellValue();
        protected abstract object GetCellValue(SerializedProperty cell);
        protected abstract void SetValue(SerializedProperty cell, object obj);

        #endregion

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            thisProperty = property;

            // Initialize properties
            GetHexRadiusProperty(property);
            GetCellSizeProperty(property);
            GetCellsProperty(property);

            // Don't draw anything if we miss a property
            if (hexRadiusProperty == null || cellSizeProperty == null || rowsProperty == null)
                return;

            // Initialize cell size to default value if not already done
            if (cellSizeProperty.vector2IntValue == default)
                cellSizeProperty.vector2IntValue = GetDefaultCellSizeValue();

            position = EditorGUI.IndentedRect(position);

            // Begin property drawing
            EditorGUI.BeginProperty(position, label, property);

            // Display foldout
            var foldoutRect = new Rect(position)
            {
                height = LineHeight
            };

            // We're using EditorGUI.IndentedRect to draw our Rects, and it already takes the indentLevel into account, so we must set it to 0.
            // This allows the PropertyDrawer to handle nested variables correctly.
            // More info: https://answers.unity.com/questions/1268850/how-to-properly-deal-with-editorguiindentlevel-in.html
            EditorGUI.indentLevel = 0;

            label.tooltip = $"Radius: {hexRadiusProperty.intValue}";

            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, property.isExpanded, label,
                menuAction: ShowHeaderContextMenu);
            EditorGUI.EndFoldoutHeaderGroup();

            // Go to next line
            position.y += LineHeight;

            if (property.isExpanded)
            {
                position.y += firstLineMargin;

                DisplayGrid(position);
            }

            EditorGUI.EndProperty();
        }

        private void ShowHeaderContextMenu(Rect position)
        {
            var menu = new GenericMenu();
            menu.AddItem(Texts.reset, false, OnReset);
            menu.AddSeparator(""); // An empty string will create a separator at the top level
            menu.AddItem(Texts.resetWithValue, false, OnResetWithValue);
            menu.AddSeparator(""); // An empty string will create a separator at the top level
            menu.AddItem(Texts.changeGridSize, false, OnChangeGridSize);
            menu.AddItem(Texts.changeCellSize, false, OnChangeCellSize);
            menu.DropDown(position);
        }

        private void OnReset() => InitNewGrid(hexRadiusProperty.intValue);
        private void OnResetWithValue() => InitNewGrid(hexRadiusProperty.intValue, false);

        private void OnChangeGridSize() => EditorWindowIntField.ShowWindow(hexRadiusProperty.intValue, InitNewGridAndRestorePreviousValues, Texts.gridSizeLabel);

        // requires an EditorWindowVec2IntField!
        private void OnChangeCellSize() => EditorWindowIntField.ShowWindow(cellSizeProperty.intValue, SetNewCellSize, Texts.cellSizeLabel);

        private void SetNewCellSize(int newCellSize)
        {
            cellSizeProperty.vector2IntValue = new(newCellSize, newCellSize);
            thisProperty.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);

            GetHexRadiusProperty(property);
            GetCellSizeProperty(property);

            if (property.isExpanded)
            {
                height += firstLineMargin;

                height += Diameter * (cellSizeProperty.vector2IntValue.y + cellSpacing.y) - cellSpacing.y; // Cells lines

                height += lastLineMargin;
            }

            return height;
        }

        private void InitNewGridAndRestorePreviousValues(int newRadius)
        {
            var previousGrid = GetGridValues();
            var previousDiameter = Diameter;

            var diameter = newRadius * 2 + 1;
            InitNewGrid(newRadius);

            for (var y = 0; y < diameter; y++)
            {
                var row = GetRowAt(y);

                for (var x = 0; x < diameter; x++)
                {
                    var cell = row.GetArrayElementAtIndex(x);

                    //TODO: shift previous grid to match hex centers
                    //var sizeDiff = hexSize - previousGridSize.x;

                    if (x < previousDiameter && y < previousDiameter)
                    {
                        // DO NOT SET VALUES IF NOT IN HEX RANGE
                        // 
                        SetValue(cell, previousGrid[y][x]);
                    }
                }
            }

            thisProperty.serializedObject.ApplyModifiedProperties();
        }

        private void InitNewGrid(int newRadius, bool useDefault = true)
        {
            rowsProperty.ClearArray();

            var diameter = newRadius * 2 + 1;

            for (var y = 0; y < diameter; y++)
            {
                rowsProperty.InsertArrayElementAtIndex(y); // Insert a new row
                var row = GetRowAt(y); // Get the new row
                row.ClearArray(); // Clear it

                for (var x = 0; x < diameter; x++)
                {
                    row.InsertArrayElementAtIndex(x);

                    var cell = row.GetArrayElementAtIndex(x);

                    SetValue(cell, useDefault ? GetDefaultCellValue() : GetFallbackCellValue());
                }
            }

            hexRadiusProperty.intValue = newRadius;
            thisProperty.serializedObject.ApplyModifiedProperties();
        }

        private object[][] GetGridValues()
        {
            var arr = new object[Diameter][];

            for (var y = 0; y < Diameter; y++)
            {
                arr[y] = new object[Diameter];

                for (var x = 0; x < Diameter; x++)
                {
                    arr[y][x] = GetCellValue(GetRowAt(y).GetArrayElementAtIndex(x));
                }
            }

            return arr;
        }

        private void DisplayGrid(Rect position)
        {
            var cellRect = new Rect(position.x, position.y, cellSizeProperty.vector2IntValue.x,
                cellSizeProperty.vector2IntValue.y);

            for (var y = 0; y < Diameter; y++)
            {
                for (var x = 0; x < Diameter; x++)
                {
                    /// renders the matrix as rhombus used for hex grids
                    var rowOffest = (cellRect.width + cellSpacing.x) / 2 * y;

                    var pos = new Rect(cellRect)
                    {
                        x = cellRect.x + (cellRect.width + cellSpacing.x) * x + rowOffest,
                        y = cellRect.y + (cellRect.height + cellSpacing.y) * y
                    };

                    var centerIndex = hexRadiusProperty.intValue;

                    /// shrinks the rhombus to a hex
                    if (x + y < centerIndex || x + y >= Diameter + centerIndex)
                    {
                        EditorGUI.DrawRect(pos, Color.clear);
                        continue;
                    }
                    /// draw the center as red X
                    //else if (x == centerIndex && y == centerIndex)
                    //{
                    //    EditorGUI.DrawTextureTransparent(pos, Base64ToTexture(s_XIconString), ScaleMode.StretchToFill, 2);
                    //    continue;
                    //}

                    var property = GetRowAt(y).GetArrayElementAtIndex(x);

                    if (property.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var match = Regex.Match(property.type, @"PPtr<\$(.+)>");
                        if (match.Success)
                        {
                            var objectType = match.Groups[1].ToString();
                            var assemblyName = "UnityEngine";
                            EditorGUI.ObjectField(pos, property, Type.GetType($"{assemblyName}.{objectType}, {assemblyName}"), GUIContent.none);
                        }
                    }
                    else
                        EditorGUI.PropertyField(pos, property, GUIContent.none);
                }
            }
        }

        public static Texture2D Base64ToTexture(string base64)
        {
            var t = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            t.LoadImage(Convert.FromBase64String(base64));
            return t;
        }

        private SerializedProperty GetRowAt(int idx) => rowsProperty.GetArrayElementAtIndex(idx).FindPropertyRelative("row");

        private void TryFindPropertyRelative(SerializedProperty parent, string relativePropertyPath, out SerializedProperty prop)
        {
            prop = parent.FindPropertyRelative(relativePropertyPath);

            if (prop == null)
            {
                Debug.LogError($"Couldn't find variable \"{relativePropertyPath}\" in {parent.name}");
            }
        }
    }

    [CustomPropertyDrawer(typeof(HexGridBool))]
    public class HexGridBoolDrawer : HexGridDrawer
    {
        protected override Vector2Int GetDefaultCellSizeValue() => new(16, 16);

        protected override object GetDefaultCellValue() => false;
        protected override object GetFallbackCellValue() => true;

        protected override object GetCellValue(SerializedProperty cell) => cell.boolValue;

        protected override void SetValue(SerializedProperty cell, object obj) => cell.boolValue = (bool)obj;
    }

    public class HexGridEnumDrawer<T> : HexGridDrawer where T : Enum
    {
        protected override Vector2Int GetDefaultCellSizeValue() => new(64, 16);

        protected override object GetDefaultCellValue() => 0;
        protected override object GetFallbackCellValue() => 1;

        protected override object GetCellValue(SerializedProperty cell) => cell.enumValueIndex;

        protected override void SetValue(SerializedProperty cell, object obj) => cell.enumValueIndex = (int)obj;
    }
}