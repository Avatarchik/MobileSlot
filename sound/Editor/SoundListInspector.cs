using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using lpesign;
using lpesign.UnityEditor;

[CustomEditor(typeof(SoundList))]
public class SoundListInspector : UsableEditor<SoundList>
{
    ReorderableList _basicList;
    ReorderableList[] _groupList;

    void OnEnable()
    {
        Initialize();

        //createBasicList
        var basicProperty = serializedObject.FindProperty("basic");
        _basicList = CreateSoundSchemaList(basicProperty, "Basic");

        //createGroupList
        var groups = serializedObject.FindProperty("groups");
        var groupCount = groups.arraySize;
        _groupList = new ReorderableList[groupCount];
        for (var i = 0; i < groupCount; ++i)
        {
            var g = groups.GetArrayElementAtIndex(i);
            var soundsProperty = g.FindPropertyRelative("sounds");
            var list = CreateSoundSchemaList(soundsProperty);
            _groupList[i] = list;
        }
    }

    ReorderableList CreateSoundSchemaList(SerializedProperty property, string headerName = "")
    {
        var list = CreateReorderableList(property, headerName);
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            OnSchemaGUI(rect, element, null);
        };

        list.onAddCallback = (ReorderableList l) =>
        {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;

            SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("name").stringValue = "NEW " + index;
            element.FindPropertyRelative("clip").objectReferenceValue = null;
        };
        return list;
    }

    void OnSchemaGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        rect.y += 2;

        Rect posA = new Rect(rect.x, rect.y, 90, _singleHeight);
        Rect posB = new Rect(posA.xMax, rect.y, rect.width - posA.width, _singleHeight);

        EditorGUI.PropertyField(posA, property.FindPropertyRelative("name"), GUIContent.none);
        EditorGUI.PropertyField(posB, property.FindPropertyRelative("clip"), GUIContent.none);
    }

    public override void OnInspectorGUI()
    {
        DrawScript();

        serializedObject.Update();

        DrawBasic();
        DrawGroup();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawBasic()
    {
        _basicList.DoLayoutList();
    }

    void DrawGroup()
    {
        SerializedProperty property = serializedObject.FindProperty("groups");

        EditorGUILayout.BeginVertical("button");
        EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        for (var i = 0; i < property.arraySize; ++i)
        {
            EditorGUILayout.BeginVertical("button");
            GUILayout.Space(3);

            var element = property.GetArrayElementAtIndex(i);

            var groupName = element.FindPropertyRelative("name");
            var type = element.FindPropertyRelative("type");
            var sounds = element.FindPropertyRelative("sounds");

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 40f;
            EditorGUILayout.PropertyField(groupName, GUILayout.MinWidth(160));
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
        if (GUILayout.Button("AddGroup"))
        {
            _script.groups.Add(new SoundGroup("new Group" + _script.groups.Count));
        }

        GUI.backgroundColor = _defaultBGColor;

        EditorGUILayout.EndVertical();
    }
}

