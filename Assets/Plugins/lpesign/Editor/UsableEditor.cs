using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using System;
using System.Collections;

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
        protected void Initialize()
        {
            _script = (T)target;

            _defaultColor = GUI.color;
            _defaultBGColor = GUI.backgroundColor;

            _singleHeight = EditorGUIUtility.singleLineHeight;
        }

        void OnEnable()
        {
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
                GUI.color = Color.yellow;
                EditorGUI.LabelField(rect, headerName, EditorStyles.boldLabel);
                GUI.color = Color.white;
            };
        }
    }
}