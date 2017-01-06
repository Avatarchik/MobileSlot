using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

using lpesign;
using lpesign.UnityEditor;

[CustomEditor(typeof(SoundList))]
public class SoundListEditor : UsableEditor<SoundList>
{
    ReorderableList _basicList;
    ReorderableList[] _groupList;

    void OnEnable()
    {
        Initialize();
        CreaetReorderLists();
    }

    protected void CreaetReorderLists()
    {
        //createBasicList
        var basicProperty = serializedObject.FindProperty("basic");
        _basicList = CreateSoundSchemaList(basicProperty);

        //createGroupList
        var groups = serializedObject.FindProperty("groups");
        var groupCount = groups.arraySize;
        _groupList = new ReorderableList[groupCount];
        for (var i = 0; i < groupCount; ++i)
        {
            var g = groups.GetArrayElementAtIndex(i);
            var soundsProperty = g.FindPropertyRelative("_sounds");
            var list = CreateSoundSchemaList(soundsProperty);
            _groupList[i] = list;
        }
    }

    ReorderableList CreateSoundSchemaList(SerializedProperty property)
    {
        var list = CreateReorderableList(property, "", false);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 3;

            Rect posA = new Rect(rect.x, rect.y, 180, _singleHeight);
            Rect posB = new Rect(posA.xMax, rect.y, rect.width - posA.width, _singleHeight);

            EditorGUI.PropertyField(posA, element.FindPropertyRelative("_name"), GUIContent.none);
            EditorGUI.PropertyField(posB, element.FindPropertyRelative("_clip"), GUIContent.none);
        };

        list.onAddCallback = (ReorderableList l) =>
        {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;

            SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("_name").stringValue = "NEW " + index;
            element.FindPropertyRelative("_clip").objectReferenceValue = null;
        };
        return list;
    }

    public override void OnInspectorGUI()
    {
        Update();

        DrawScript();

        serializedObject.Update();

        DrawDefaultButton();
        DrawBasic();
        DrawGroup();
        //DrawClearAllButton();

        Apply();
    }

    void DrawDefaultButton()
    {
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Set By SlotConfig", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Set By SlotConfig?", "Are you sure you want to set all list? This action cannot be undone.", "OK", "Cancel"))
            {
                _script.CreateDefaultList();
            }
        }
        GUI.backgroundColor = _defaultBGColor;
    }

    void DrawBasic()
    {
        EditorGUILayout.LabelField("Basic", EditorStyles.boldLabel);
        _basicList.DoLayoutList();
    }

    void DrawGroup()
    {

        // EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
        var groups = BeginFoldout("groups", "Groups");
        if (groups.isExpanded)
        {
            // EditorGUILayout.BeginVertical(GUI.skin.button);
            SerializedProperty property = serializedObject.FindProperty("groups");
            for (var i = 0; i < property.arraySize; ++i)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Space(3);

                var element = property.GetArrayElementAtIndex(i);

                var groupName = element.FindPropertyRelative("_name");
                var type = element.FindPropertyRelative("type");

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 60f;
                EditorGUILayout.PropertyField(groupName, GUILayout.MinWidth(200));
                EditorGUILayout.PropertyField(type, GUILayout.MinWidth(110));
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X"))
                {
                    if (EditorUtility.DisplayDialog("Delete " + groupName + "?", "Are you sure you want to delete " + groupName + "? This action cannot be undone.", "Delete", "Cancel"))
                    {
                        _script.groups.RemoveAt(i);
                    }
                }
                GUI.backgroundColor = _defaultBGColor;
                EditorGUILayout.EndHorizontal();

                var list = _groupList[i];
                list.DoLayoutList();

                EditorGUILayout.EndVertical();
            }
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Add Group", GUILayout.Height(30)))
            {
                _script.groups.Add(new SoundGroup("new Group" + _script.groups.Count));
            }
            GUI.backgroundColor = _defaultBGColor;

            // EditorGUILayout.EndVertical();
        }
        EndFoldOut();
    }

    void DrawClearAllButton()
    {
        GUILayout.Space(10);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("AllClear", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear All Sounds?", "Are you sure you want to clear all sounds? This action cannot be undone.", "Clear All", "Cancel"))
            {
                _script.Clear();
            }
        }
        GUI.backgroundColor = _defaultBGColor;
    }
}

