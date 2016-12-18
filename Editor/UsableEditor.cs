using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace lpesign.UnityEditor
{

    public class UsableEditor<T> : Editor where T : MonoBehaviour
    {
        protected T _script;
        protected MonoScript _mono;

        protected void FindScript()
        {
            _script = (T)target;
        }

        protected void DrawScript()
        {
            if (_script == null) FindScript();

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

        // 1. public ReorderableList(IList elements, Type elementType),
        // 2. public ReorderableList(SerializedObject serializedObject, SerializedProperty elements).
        protected ReorderableList CreateReorderableList(string propertyName,
                                                        string HeaderName = "",
                                                        bool draggable = true,
                                                        bool displayAddButton = true,
                                                        bool displayRemoveButton = true)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            return CreateReorderableList(property, HeaderName);
        }

        protected ReorderableList CreateReorderableList(SerializedProperty property,
                                                        string HeaderName = "",
                                                        bool draggable = true,
                                                        bool displayAddButton = true,
                                                        bool displayRemoveButton = true)
        {
            bool displayHeader = !string.IsNullOrEmpty(HeaderName);

            var list = new ReorderableList(serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton);
            if (displayHeader)
            {
                list.drawHeaderCallback = (Rect rect) =>
                {
                    GUI.color = Color.yellow;
                    EditorGUI.LabelField(rect, HeaderName, EditorStyles.boldLabel);
                    GUI.color = Color.white;
                };
            }
            return list;
        }
    }
}