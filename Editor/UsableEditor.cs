using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using lpesign;

namespace lpesign.UnityEditor
{
    public abstract class UsableEditor<T> : Editor where T : MonoBehaviour
    {
        protected T _script;

        protected MonoScript _mono;
        protected Color _defaultBGColor;
        protected Color _defaultColor;
        protected float _singleHeight;

        /// <summary>
        /// Initialize is must called when OnEnable
        /// </summary>
        protected void Initialize()
        {
            // EditorGUI.indentLevel++;
            // EditorGUIUtility.labelWidth = 250;
            // EditorGUIUtility.fieldWidth = 150;

            _script = (T)target;

            _defaultColor = GUI.color;
            _defaultBGColor = GUI.backgroundColor;

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

        protected void DrawRowLine(string lineName = "")
        {
            EditorGUILayout.Space();
            GUI.color = Color.white;
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
            if (string.IsNullOrEmpty(lineName) == false) GUILayout.Label(lineName, EditorStyles.boldLabel);
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
                    EditorGUILayout.IntField((int)propertyValue);
                    // prop.SetPropertyValue( EditorGUILayout.Slider((int)prop.GetPropertyValue(), 2, 10));
                    break;

                case SerializedPropertyType.Float:
                    EditorGUILayout.FloatField((float)propertyValue);
                    break;

                case SerializedPropertyType.String:
                    EditorGUILayout.TextField((string)propertyValue);
                    break;

                case SerializedPropertyType.Boolean:
                    prop.SetPropertyValue(EditorGUILayout.Toggle((bool)propertyValue));
                    break;

                case SerializedPropertyType.Enum:
                    EditorGUILayout.PropertyField(prop, GUIContent.none);
                    break;

                case SerializedPropertyType.ObjectReference:
                    EditorGUILayout.PropertyField(prop, GUIContent.none);
                    // Debug.LogWarning("ObjectReference Type not yet supported");
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
        protected SerializedProperty BeginFoldout(SerializedProperty targetProperty, string propertyName, string label = "")
        {
            GUIStyle foldStyle = EditorStyles.foldout;
            FontStyle previousStyle = foldStyle.fontStyle;
            foldStyle.fontStyle = FontStyle.Bold;

            var property = targetProperty.FindPropertyRelative(propertyName);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            label = StringUtil.IsNullOrEmpty(label) ? propertyName : label;
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, foldStyle);

            foldStyle.fontStyle = previousStyle;
            return property;
        }

        protected SerializedProperty BeginFoldout(string propertyName, string label = "")
        {
            GUIStyle foldStyle = EditorStyles.foldout;
            FontStyle previousStyle = foldStyle.fontStyle;
            foldStyle.fontStyle = FontStyle.Bold;

            var property = serializedObject.FindProperty(propertyName);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            label = StringUtil.IsNullOrEmpty(label) ? propertyName : label;
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, foldStyle);

            foldStyle.fontStyle = previousStyle;
            return property;
        }

        protected void EndFoldOut()
        {
            EditorGUI.indentLevel--;
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
    }
}