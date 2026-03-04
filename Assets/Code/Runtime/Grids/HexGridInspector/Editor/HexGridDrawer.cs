using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Code.Runtime.Grids.HexGridInspector.Editor
{
    public abstract class HexGridDrawer : PropertyDrawer
    {
        private static float LineHeight => EditorGUIUtility.singleLineHeight;

        private const float firstLineMargin = 5f;
        private const float lastLineMargin  = 2f;

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
            public static readonly GUIContent reset           = new("Reset");
            public static readonly GUIContent resetWithValue  = new("Reset with value");
            public static readonly GUIContent changeGridSize  = new("Change Hex Radius");
            public static readonly GUIContent changeCellSize  = new("Change Cell Size");

            public const string gridSizeLabel = "Hex Radius";
            public const string cellSizeLabel = "Cell Size";
        }

        #endregion

        #region Abstract and virtual methods

        protected virtual Vector2Int GetDefaultCellSizeValue() => new(32, 16);

        protected abstract object GetDefaultCellValue();
        protected abstract object GetFallbackCellValue();
        protected abstract object GetCellValue(SerializedProperty cell);
        protected abstract void   SetValue(SerializedProperty cell, object obj);

        #endregion

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            thisProperty = property;

            GetHexRadiusProperty(property);
            GetCellSizeProperty(property);
            GetCellsProperty(property);

            if (hexRadiusProperty == null || cellSizeProperty == null || rowsProperty == null)
                return;

            // Auto-initialize if the rows array is empty or size-mismatched —
            // happens on fresh instances where Unity skips the parameterized constructor.
            if (rowsProperty.arraySize != hexRadiusProperty.intValue * 2 + 1)
                InitNewGrid(hexRadiusProperty.intValue);

            if (cellSizeProperty.vector2IntValue == default)
                cellSizeProperty.vector2IntValue = GetDefaultCellSizeValue();

            position = EditorGUI.IndentedRect(position);

            EditorGUI.BeginProperty(position, label, property);

            var foldoutRect = new Rect(position) { height = LineHeight };

            EditorGUI.indentLevel = 0;

            label.tooltip = $"Radius: {hexRadiusProperty.intValue}";

            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, property.isExpanded, label,
                menuAction: ShowHeaderContextMenu);
            EditorGUI.EndFoldoutHeaderGroup();

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
            menu.AddItem(Texts.reset,          false, OnReset);
            menu.AddSeparator("");
            menu.AddItem(Texts.resetWithValue, false, OnResetWithValue);
            menu.AddSeparator("");
            menu.AddItem(Texts.changeGridSize, false, OnChangeGridSize);
            menu.AddItem(Texts.changeCellSize, false, OnChangeCellSize);
            menu.DropDown(position);
        }

        private void OnReset()         => InitNewGrid(hexRadiusProperty.intValue);
        private void OnResetWithValue() => InitNewGrid(hexRadiusProperty.intValue, false);

        private void OnChangeGridSize() =>
            EditorWindowIntField.ShowWindow(hexRadiusProperty.intValue, InitNewGridAndRestorePreviousValues, Texts.gridSizeLabel);

        private void OnChangeCellSize() =>
            EditorWindowIntField.ShowWindow(cellSizeProperty.intValue, SetNewCellSize, Texts.cellSizeLabel);

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
                height += Diameter * (cellSizeProperty.vector2IntValue.y + cellSpacing.y) - cellSpacing.y;
                height += lastLineMargin;
            }

            return height;
        }

        private void InitNewGridAndRestorePreviousValues(int newRadius)
        {
            var previousGrid     = GetGridValues();
            var previousDiameter = Diameter;
            var diameter         = newRadius * 2 + 1;

            InitNewGrid(newRadius);

            for (var y = 0; y < diameter; y++)
            {
                var row = GetRowAt(y);
                for (var x = 0; x < diameter; x++)
                {
                    var cell = row.GetArrayElementAtIndex(x);
                    if (x < previousDiameter && y < previousDiameter)
                        SetValue(cell, previousGrid[y][x]);
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
                rowsProperty.InsertArrayElementAtIndex(y);
                var row = GetRowAt(y);
                row.ClearArray();

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
                    arr[y][x] = GetCellValue(GetRowAt(y).GetArrayElementAtIndex(x));
            }
            return arr;
        }

        private void DisplayGrid(Rect position)
        {
            var cellRect = new Rect(position.x, position.y,
                cellSizeProperty.vector2IntValue.x,
                cellSizeProperty.vector2IntValue.y);

            for (var y = 0; y < Diameter; y++)
            {
                for (var x = 0; x < Diameter; x++)
                {
                    var rowOffset = (cellRect.width + cellSpacing.x) / 2 * y;
                    var pos = new Rect(cellRect)
                    {
                        x = cellRect.x + (cellRect.width + cellSpacing.x) * x + rowOffset,
                        y = cellRect.y + (cellRect.height + cellSpacing.y) * y
                    };

                    var centerIndex = hexRadiusProperty.intValue;

                    if (x + y < centerIndex || x + y >= Diameter + centerIndex)
                    {
                        EditorGUI.DrawRect(pos, Color.clear);
                        continue;
                    }

                    var prop = GetRowAt(y).GetArrayElementAtIndex(x);

                    if (prop.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        var match = Regex.Match(prop.type, @"PPtr<\$(.+)>");
                        if (match.Success)
                        {
                            var objectType   = match.Groups[1].ToString();
                            var assemblyName = "UnityEngine";
                            EditorGUI.ObjectField(pos, prop,
                                Type.GetType($"{assemblyName}.{objectType}, {assemblyName}"), GUIContent.none);
                        }
                    }
                    else
                        EditorGUI.PropertyField(pos, prop, GUIContent.none);
                }
            }
        }

        public static Texture2D Base64ToTexture(string base64)
        {
            var t = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            t.LoadImage(Convert.FromBase64String(base64));
            return t;
        }

        private SerializedProperty GetRowAt(int idx) =>
            rowsProperty.GetArrayElementAtIndex(idx).FindPropertyRelative("row");

        private void TryFindPropertyRelative(SerializedProperty parent, string relativePropertyPath,
            out SerializedProperty prop)
        {
            prop = parent.FindPropertyRelative(relativePropertyPath);
            if (prop == null)
                Debug.LogError($"Couldn't find variable \"{relativePropertyPath}\" in {parent.name}");
        }
    }

    [CustomPropertyDrawer(typeof(HexGridBool))]
    public class HexGridBoolDrawer : HexGridDrawer
    {
        protected override Vector2Int GetDefaultCellSizeValue() => new(16, 16);

        protected override object GetDefaultCellValue()  => false;
        protected override object GetFallbackCellValue() => true;

        protected override object GetCellValue(SerializedProperty cell) => cell.boolValue;

        protected override void SetValue(SerializedProperty cell, object obj) => cell.boolValue = (bool)obj;
    }

    public class HexGridEnumDrawer<T> : HexGridDrawer where T : Enum
    {
        protected override Vector2Int GetDefaultCellSizeValue() => new(64, 16);

        protected override object GetDefaultCellValue()  => 0;
        protected override object GetFallbackCellValue() => 1;

        protected override object GetCellValue(SerializedProperty cell) => cell.enumValueIndex;

        protected override void SetValue(SerializedProperty cell, object obj) => cell.enumValueIndex = (int)obj;
    }
}