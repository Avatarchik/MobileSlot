using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;

using lpesign.UnityEditor;

namespace Game
{
    [CustomEditor(typeof(SlotConfig))]
    public class SlotConfigEditor : UsableEditor<SlotConfig>
    {
        SlotCreator _creator;
        void OnEnable()
        {
            Initialize();

            _creator = FindObjectOfType<SlotCreator>();

            if (_creator != null)
            {
                _script.name = _creator.gameObject.name;
            }
        }

        public override void OnInspectorGUI()
        {
            DrawScript();
            DrawScriptSetting();
            DrawBase();
            DrawBetting();

            Apply();

            //-- return
            DrawRowLine("BASE");
            DrawDefaultInspector();
        }

        void DrawScriptSetting()
        {
            if (_creator == null) return;

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("SetByCreator", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("SetByCreator?", "Are you sure you want to set all list? This action cannot be undone.", "OK", "Cancel"))
                {
                    _creator.SettingByScript();
                }
            }
            GUI.backgroundColor = _defaultBGColor;
        }

        void DrawBase()
        {
            EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);
            GUILayout.BeginVertical(GUI.skin.button, GUILayout.ExpandWidth(true));

            DrawHorizontalProperties("name", "ID");
            DrawHorizontalProperties("Host", "Port");
            DrawHorizontalProperties("Version");

            GUILayout.EndVertical();
        }


        ReorderableList _bettingList;

        void DrawBetting()
        {
            var betting = serializedObject.FindProperty("Betting");

            EditorGUILayout.LabelField("Betting", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                DrawHorizontalProperties(betting, 100f, "_currentLineBet", "_lastLineBet");
                EditorGUILayout.EndVertical();
            }

            if (_bettingList == null)
            {
                var table = betting.FindPropertyRelative("_betTable");
                _bettingList = CreateReorderableList(table);
                _bettingList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    rect.height -= 5F;
                    var element = _bettingList.serializedProperty.GetArrayElementAtIndex(index);
                    var GUIContent = new GUIContent("Bet " + (index + 1));
                    EditorGUI.PropertyField(rect, element, GUIContent);
                };
                _bettingList.onCanRemoveCallback = (ReorderableList l) =>
                {
                    return l.count > 1;
                };
            }

            _bettingList.DoLayoutList();

            // list.onAddCallback = (ReorderableList l) =>
            // {
            //     var index = l.serializedProperty.arraySize;
            //     l.serializedProperty.arraySize++;
            //     l.index = index;

            //     SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
            //     element.FindPropertyRelative("name").stringValue = "NEW " + index;
            //     element.FindPropertyRelative("clip").objectReferenceValue = null;
            // };
            // return list;

            // GUILayout.EndVertical();
        }
    }
}

