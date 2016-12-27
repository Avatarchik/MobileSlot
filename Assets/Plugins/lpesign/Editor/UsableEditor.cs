using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using lpesign;

namespace lpesign.UnityEditor
{

    public class UsableEditor<T> : Editor where T : MonoBehaviour
    {
        protected T _script;

        protected MonoScript _mono;
        protected Color _defaultBGColor;
        protected Color _defaultColor;
        protected float _singleHeight;

        /// <summary>
        /// Initialize is must called when OnEnable
        /// </summary>
        virtual protected void Initialize()
        {
            // EditorGUI.indentLevel++;
            // EditorGUIUtility.labelWidth = 250;
            // EditorGUIUtility.fieldWidth = 150;

            _script = (T)target;

            _defaultColor = GUI.color;
            _defaultBGColor = GUI.backgroundColor;

            _singleHeight = EditorGUIUtility.singleLineHeight;
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
        protected void DrawHorizontalProperties(params string[] properties)
        {
            DrawHorizontalProperties(null, 50f, properties);
        }

        protected void DrawHorizontalProperties(SerializedProperty targetProperty, params string[] properties)
        {
            DrawHorizontalProperties(targetProperty, 50f, properties);
        }

        protected void DrawHorizontalProperties(float labelWidth, params string[] properties)
        {
            DrawHorizontalProperties(null, labelWidth, properties);
        }

        protected void DrawHorizontalProperties(SerializedProperty targetProperty, float labelWidth, params string[] properties)
        {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < properties.Length; ++i)
            {
                if (targetProperty == null) DrawPropertyField(properties[i], labelWidth);
                else DrawPropertyField(targetProperty, properties[i], labelWidth);
            }
            GUILayout.EndHorizontal();
        }

        protected void DrawPropertyField(string propertyName, float labelWidth = 50f)
        {
            var prop = serializedObject.FindProperty(propertyName);
            DrawPropertyField(prop, labelWidth);
        }

        protected void DrawPropertyField(SerializedProperty targetProperty, string propertyName, float labelWidth = 50f)
        {
            var prop = targetProperty.FindPropertyRelative(propertyName);
            DrawPropertyField(prop, labelWidth);
        }

        protected void DrawPropertyField(SerializedProperty prop, float labelWidth = 50f)
        {
            var propertyValue = prop.GetPropertyValue();

            GUILayout.Label(prop.displayName, GUILayout.Width(labelWidth));
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    EditorGUILayout.IntField((int)propertyValue);
                    break;

                case SerializedPropertyType.Float:
                    EditorGUILayout.FloatField((float)propertyValue);
                    break;

                case SerializedPropertyType.String:
                    EditorGUILayout.TextField((string)propertyValue);
                    break;

                case SerializedPropertyType.Boolean:
                    break;

                case SerializedPropertyType.ObjectReference:
                    break;

                case SerializedPropertyType.LayerMask:
                    break;

                case SerializedPropertyType.Enum:
                    break;

                case SerializedPropertyType.Vector2:
                    break;

                case SerializedPropertyType.Vector3:
                    break;

                case SerializedPropertyType.Vector4:
                    break;

                case SerializedPropertyType.Rect:
                    break;

                case SerializedPropertyType.ArraySize:
                    break;

                case SerializedPropertyType.Character:
                    break;

                case SerializedPropertyType.AnimationCurve:
                    break;

                case SerializedPropertyType.Bounds:
                    break;

                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
            }
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