using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using lpesign;
using lpesign.UnityEditor;

[CustomEditor(typeof(SoundList))]
public class SoundListInspector : UsableEditor<SoundList>
{
    float _singleHeight;
    ReorderableList _basicList;

    void OnEnable()
    {
        FindScript();

        _singleHeight = EditorGUIUtility.singleLineHeight;

        _basicList = CreateReorderableList("basic", "Basic Category");
        _basicList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = _basicList.serializedProperty.GetArrayElementAtIndex(index);
            OnSchemaGUi(rect, element, null);
        };

        _basicList.onRemoveCallback = (ReorderableList l) =>
        {
            if (EditorUtility.DisplayDialog("Really?", "delete", "Yes", "No"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };
        _basicList.onAddCallback = (ReorderableList l) =>
        {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;

            SerializedProperty element = l.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("name").stringValue = "NEW " + index;
            element.FindPropertyRelative("clip").objectReferenceValue = null;
        };
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DrawRowLine("Custom Inspector");

        DrawScript();

        serializedObject.Update();

        _basicList.DoLayoutList();

        DrawGroup();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawGroup()
    {
        EditorGUILayout.BeginVertical("button");
        var categories = _script.categories;
        for (var i = 0; i < categories.Length; ++i)
        {
            var category = categories[i];
            EditorGUILayout.LabelField(category.name, EditorStyles.boldLabel, GUILayout.MaxWidth(130));
            _basicList.DoLayoutList();
        }
        EditorGUILayout.EndVertical();

        /*
        GUILayoutOption[] options = { GUILayout.ExpandWidth(true) };
        EditorGUILayout.BeginVertical("button", options);
        EditorGUILayout.LabelField("test1", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.LabelField("test2", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("test1", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.LabelField("test2", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("button");
        EditorGUILayout.LabelField("test3", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.LabelField("test4", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.EndVertical();

		DrawRowLine("horizontal");

        EditorGUILayout.BeginHorizontal("button");
        EditorGUILayout.LabelField("test3", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.LabelField("test4", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("test3", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.LabelField("test4", EditorStyles.boldLabel, GUILayout.MaxWidth(130));
        EditorGUILayout.EndHorizontal();
		*/
    }

    public void OnSchemaGUi(Rect rect, SerializedProperty property, GUIContent label)
    {
        rect.y += 2;

        Rect posA = new Rect(rect.x, rect.y, 90, _singleHeight);
        Rect posB = new Rect(posA.xMax, rect.y, rect.width - posA.width, _singleHeight);

        EditorGUI.PropertyField(posA, property.FindPropertyRelative("name"), GUIContent.none);
        EditorGUI.PropertyField(posB, property.FindPropertyRelative("clip"), GUIContent.none);
    }
}

