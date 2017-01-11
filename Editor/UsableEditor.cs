using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace lpesign.UnityEditor
{
    public abstract class UsableEditor<T> : Editor where T : MonoBehaviour
    {
        protected T _script;

        protected MonoScript _mono;
        protected Color _defaultBGColor;
        protected Color _defaultColor;
        protected float _singleHeight;
        protected GUIStyle _defaultStye;

        /// <summary>
        /// Initialize is must called when OnEnable
        /// </summary>
        protected void Initialize()
        {
            // EditorGUI.indentLevel++;
            // EditorGUIUtility.labelWidth = 250;
            // EditorGUIUtility.fieldWidth = 150;
            // GUILayout.FlexibleSpace();
            // GUILayout.Space(5);
            // "∙"

            _script = (T)target;

            _defaultColor = GUI.color;
            _defaultBGColor = GUI.backgroundColor;

            _defaultStye = new GUIStyle();

            _singleHeight = EditorGUIUtility.singleLineHeight;
        }

        protected void Update()
        {
            serializedObject.Update();
        }

        protected void Apply()
        {
            serializedObject.ApplyModifiedProperties();
        }

        protected GUIStyle GetColorStyle(Color color)
        {
            var style = new GUIStyle();
            Texture2D texture = new Texture2D(16, 16);
            for (int y = 0; y < texture.height; ++y)
            {
                for (int x = 0; x < texture.width; ++x)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            style.normal.background = texture;

            return style;
        }

        protected void DrawScript()
        {
            if (_script == null) Initialize();

            GUI.enabled = false;
            _mono = EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(_script), typeof(T), false) as MonoScript;
            GUI.enabled = true;
            EditorGUILayout.Space();
        }

        protected void DrawRowLine(float w = 0f, float h = 1f)
        {
            GUI.color = Color.white;
            GUILayout.Box("", w > 0 ? GUILayout.Width(w) : GUILayout.ExpandWidth(true), GUILayout.Height(h));
            GUI.color = _defaultColor;
        }

        #region DrawProperty
        protected SerializedProperty DrawMultiPropertyFields(params string[] properties)
        {
            return DrawHorizontalProperties(null, 60f, properties);
        }

        protected SerializedProperty DrawHorizontalProperties(float labelWidth, params string[] properties)
        {
            return DrawHorizontalProperties(null, labelWidth, properties);
        }

        protected SerializedProperty DrawHorizontalProperties(SerializedProperty targetProperty, params string[] properties)
        {
            return DrawHorizontalProperties(targetProperty, 60f, properties);
        }

        protected SerializedProperty DrawHorizontalProperties(SerializedProperty targetProperty, float labelWidth, params string[] properties)
        {
            SerializedProperty resProp = null;
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < properties.Length; ++i)
            {
                if (targetProperty == null)
                {
                    resProp = DrawPropertyField(properties[i], labelWidth);
                }
                else
                {
                    resProp = DrawPropertyField(targetProperty, labelWidth, properties[i]);
                }
                if (i < properties.Length - 1) GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            return resProp;
        }

        protected SerializedProperty DrawPropertyField(string propertyName, float labelWidth = 60f)
        {
            var prop = serializedObject.FindProperty(propertyName);
            return DrawPropertyField(prop, labelWidth);
        }

        protected SerializedProperty DrawPropertyField(SerializedProperty targetProperty, string propertyName)
        {
            var prop = targetProperty.FindPropertyRelative(propertyName);
            return DrawPropertyField(prop, 60f);
        }

        protected SerializedProperty DrawPropertyField(SerializedProperty targetProperty, float labelWidth, string propertyName)
        {
            var prop = targetProperty.FindPropertyRelative(propertyName);
            return DrawPropertyField(prop, labelWidth);
        }

        protected SerializedProperty DrawPropertyField(SerializedProperty prop, float labelWidth = 60f)
        {
            var propertyValue = prop.GetPropertyValue();
            var propertyType = prop.propertyType;

            //custom property
            if (propertyType == SerializedPropertyType.Generic)
            {
                EditorGUILayout.PropertyField(prop, prop.hasChildren);
                return prop;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(prop.displayName, GUILayout.Width(labelWidth));
            switch (propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = EditorGUILayout.IntField((int)propertyValue);
                    break;

                case SerializedPropertyType.Float:
                    prop.floatValue = EditorGUILayout.FloatField((float)propertyValue);
                    break;

                case SerializedPropertyType.String:
                    prop.stringValue = EditorGUILayout.TextField((string)propertyValue);
                    break;

                case SerializedPropertyType.Boolean:
                    prop.boolValue = EditorGUILayout.Toggle((bool)propertyValue);
                    // prop.boolValue = prop.SetPropertyValue(EditorGUILayout.Toggle((bool)propertyValue));
                    break;

                case SerializedPropertyType.Enum:
                    prop.SetEnumValue(EditorGUILayout.PropertyField(prop, GUIContent.none));
                    break;

                case SerializedPropertyType.ObjectReference:
                    EditorGUILayout.PropertyField(prop, GUIContent.none);
                    break;

                case SerializedPropertyType.LayerMask:
                    Debug.LogWarning("LayerMask Type not yet supported");
                    break;

                case SerializedPropertyType.Vector2:
                    Debug.LogWarning("Vector2 Type not yet supported");
                    break;

                case SerializedPropertyType.Vector3:
                    Debug.LogWarning("Vector3 Type not yet supported");
                    break;

                case SerializedPropertyType.Vector4:
                    Debug.LogWarning("Vector4 Type not yet supported");
                    break;

                case SerializedPropertyType.Rect:
                    Debug.LogWarning("Rect Type not yet supported");
                    break;

                case SerializedPropertyType.ArraySize:
                    Debug.LogWarning("ArraySize Type not yet supported");
                    break;

                case SerializedPropertyType.Character:
                    Debug.LogWarning("Character Type not yet supported");
                    break;

                case SerializedPropertyType.AnimationCurve:
                    Debug.LogWarning("AnimationCurve Type not yet supported");
                    break;

                case SerializedPropertyType.Bounds:
                    Debug.LogWarning("Bounds Type not yet supported");
                    break;

                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");

                default:
                    Debug.LogWarning(prop.propertyType + " Type is not defined");
                    break;
            }

            EditorGUILayout.EndHorizontal();
            return prop;
        }
        #endregion

        #region foldout
        protected SerializedProperty BeginFoldout(SerializedProperty targetProperty, string propertyName, string label = "", string areaStyle = "box")
        {
            GUIStyle foldStyle = EditorStyles.foldout;
            FontStyle previousStyle = foldStyle.fontStyle;
            foldStyle.fontStyle = FontStyle.Bold;

            var property = targetProperty.FindPropertyRelative(propertyName);
            EditorGUILayout.BeginVertical(areaStyle == "" ? _defaultStye : areaStyle);
            label = StringUtil.IsNullOrEmpty(label) ? propertyName : label;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, foldStyle);
            EditorGUILayout.EndHorizontal();

            foldStyle.fontStyle = previousStyle;
            return property;
        }

        protected SerializedProperty BeginFoldout(string propertyName, string label = "", string areaStyle = "box")
        {
            GUIStyle foldStyle = EditorStyles.foldout;
            FontStyle previousStyle = foldStyle.fontStyle;
            foldStyle.fontStyle = FontStyle.Bold;

            var property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.BeginVertical(areaStyle == "" ? _defaultStye : areaStyle);
            label = StringUtil.IsNullOrEmpty(label) ? propertyName : label;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15);
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, foldStyle);
            EditorGUILayout.EndHorizontal();

            foldStyle.fontStyle = previousStyle;
            return property;
        }

        protected void EndFoldOut()
        {
            EditorGUILayout.EndVertical();
        }


        #endregion

        #region ReorderableList
        //----------------------------------------------------------------------------------------------
        // Create ReorderableList
        //drawElementCallback       (Rect rect, int index, bool isActive, bool isFocused)
        //drawHeaderCallback        (Rect rect)
        //onReorderCallback         (ReorderableList list)
        //onSelectCallback          (ReorderableList list)
        //onAddCallback             (ReorderableList list)
        //onAddDropdownCallback     (Rect buttonRect, ReorderableList list)
        //onRemoveCallback          (ReorderableList list)
        //onCanRemoveCallback bool  (ReorderableList list)
        //onChangedCallback         (ReorderableList list)
        //----------------------------------------------------------------------------------------------
        protected ReorderableList CreateReorderableList(string propertyName,
                                                        string HeaderName = "",
                                                        bool draggable = true,
                                                        bool displayAddButton = true,
                                                        bool displayRemoveButton = true)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return CreateReorderableList(property, HeaderName, draggable, displayAddButton, displayRemoveButton);
        }

        protected ReorderableList CreateReorderableList(SerializedProperty property,
                                                        string headerName = "",
                                                        bool draggable = true,
                                                        bool displayAddButton = true,
                                                        bool displayRemoveButton = true)
        {
            bool displayHeader = !string.IsNullOrEmpty(headerName);

            var list = new ReorderableList(serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton);
            if (displayHeader) AddDrawHeaderCallbackToReorderList(ref list, headerName);
            return list;
        }

        void AddDrawHeaderCallbackToReorderList(ref ReorderableList list, string headerName)
        {
            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, headerName, EditorStyles.boldLabel);
            };
        }
        #endregion

        #region Handle Array
        protected int DrawStringPopup(int selectedValue, string[] names, float w = 30)
        {
            var indices = new int[names.Length];
            for (var i = 0; i < names.Length; ++i) indices[i] = i;

            return EditorGUILayout.IntPopup(selectedValue, names, indices, GUILayout.Width(w));
        }

        protected string DrawStringPopup(string selectedValue, string[] names, float w = 30)
        {
            var indices = new int[names.Length];
            for (var i = 0; i < names.Length; ++i) indices[i] = i;

            var selectedIndex = names.IndexOf(selectedValue);
            if (selectedIndex == -1) throw new System.ArgumentException("selectedValue must be included in names");

            selectedIndex = EditorGUILayout.IntPopup(selectedIndex, names, indices, GUILayout.Width(w));

            return names[selectedIndex];
        }

        protected void DrawIntArrayBox(SerializedProperty prop, int range, float w = 30)
        {
            string[] names;
            int[] values;
            EditorExtension.GetIntNameAndValue(out names, out values, range);

            for (var i = 0; i < prop.arraySize; ++i)
            {
                var element = prop.GetArrayElementAtIndex(i);
                element.intValue = EditorGUILayout.IntPopup(element.intValue, names, values, GUILayout.Width(w));
            }
        }

        protected void DrawAudioClipArray(SerializedProperty prop, float w = 30)
        {
            for (var i = 0; i < prop.arraySize; ++i)
            {
                var element = prop.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, GUIContent.none);
            }
        }
        #endregion
    }


    public static class EditorExtension
    {
        public static void GetIntNameAndValue(out string[] names, out int[] values, int range)
        {
            names = new string[range];
            values = new int[range];

            for (var i = 0; i < range; ++i)
            {
                names[i] = i.ToString();
                values[i] = i;
            }
        }

        public static void SetEnumValue(this SerializedProperty prop, object value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if (value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

            var tp = value.GetType();
            if (tp.IsEnum)
            {
                int i = prop.enumNames.IndexOf(System.Enum.GetName(tp, value));
                if (i < 0) i = 0;
                prop.enumValueIndex = i;
            }
            else
            {
                int i = ConvertUtil.ToInt(value);
                if (i < 0 || i >= prop.enumNames.Length) i = 0;
                prop.enumValueIndex = i;
            }
        }

        public static System.Enum GetEnumValue(this SerializedProperty prop, System.Type tp)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!tp.IsEnum) throw new System.ArgumentException("Type must be an enumerated type.");

            try
            {
                var name = prop.enumNames[prop.enumValueIndex];
                return System.Enum.Parse(tp, name) as System.Enum;
            }
            catch
            {
                return System.Enum.GetValues(tp).Cast<System.Enum>().First();
            }
        }

        public static System.Enum GetEnumValue<T>(this SerializedProperty prop)
        {
            var tp = typeof(T);
            if (!tp.IsEnum) throw new System.ArgumentException("T must be an enumerated type");

            try
            {
                var name = prop.enumNames[prop.enumValueIndex];
                return System.Enum.Parse(tp, name) as System.Enum;
            }
            catch
            {
                return System.Enum.GetValues(tp).Cast<System.Enum>().First();
            }
        }

        public static void SetPropertyValue(this SerializedProperty prop, object value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = ConvertUtil.ToInt(value);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = ConvertUtil.ToBool(value);
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = ConvertUtil.ToSingle(value);
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = ConvertUtil.ToString(value);
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = (Color)value;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = value as Object;
                    break;
                case SerializedPropertyType.LayerMask:
                    prop.intValue = (value is LayerMask) ? ((LayerMask)value).value : ConvertUtil.ToInt(value);
                    break;
                case SerializedPropertyType.Enum:
                    //prop.enumValueIndex = ConvertUtil.ToInt(value);
                    prop.SetEnumValue(value);
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = (Vector2)value;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = (Vector3)value;
                    break;
                case SerializedPropertyType.Vector4:
                    prop.vector4Value = (Vector4)value;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = (Rect)value;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = ConvertUtil.ToInt(value);
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = ConvertUtil.ToInt(value);
                    break;
                case SerializedPropertyType.AnimationCurve:
                    prop.animationCurveValue = value as AnimationCurve;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = (Bounds)value;
                    break;
                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
            }
        }
        public static object GetPropertyValue(this SerializedProperty prop)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    return (LayerMask)prop.intValue;
                case SerializedPropertyType.Enum:
                    return prop.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize;
                case SerializedPropertyType.Character:
                    return (char)prop.intValue;
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
            }

            return null;
        }
    }
}