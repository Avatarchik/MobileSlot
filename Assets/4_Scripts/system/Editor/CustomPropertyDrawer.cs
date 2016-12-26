using UnityEngine;

using UnityEditor;

using Game;
using lpesign;

[CustomPropertyDrawer(typeof(Size2D))]
public class Size2DDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        Rect contentPos = EditorGUI.PrefixLabel(position, label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;


        EditorGUIUtility.labelWidth = 15f;
        Rect wRect = new Rect(contentPos.x, contentPos.y, contentPos.width * 0.5f, contentPos.height);
        Rect hRect = new Rect(wRect.x + wRect.width, wRect.y, contentPos.width * 0.5f, contentPos.height);

        EditorGUI.PropertyField(wRect, property.FindPropertyRelative("width"), new GUIContent("W"));
        EditorGUI.PropertyField(hRect, property.FindPropertyRelative("height"), new GUIContent("H"));

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(MoveTweenInfo))]
public class MoveTweenInfoDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(rect, label, property);
        Rect contentPos = EditorGUI.PrefixLabel(rect, label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        EditorGUIUtility.labelWidth = 40f;

        Rect wRect = new Rect(contentPos.x, contentPos.y, contentPos.width * 0.5f, contentPos.height);
        Rect hRect = new Rect(wRect.x + wRect.width, wRect.y, contentPos.width * 0.5f, contentPos.height);

        EditorGUI.PropertyField(wRect, property.FindPropertyRelative("distance"), new GUIContent("dis"));
        EditorGUI.PropertyField(hRect, property.FindPropertyRelative("duration"), new GUIContent("time"));

        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}

// [CustomPropertyDrawer(typeof(MyDictionary))]
// public class MyDictionaryDrawer : SerializableDictionaryDrawer<string, int> { }

// [CustomPropertyDrawer(typeof(ReelStripList))]
// public class ReelStripBundleDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
//     {
//         label = EditorGUI.BeginProperty(rect, label, property);

//         var map = property.FindPropertyRelative("_reelStrips");

//         Debug.Log("map" + (map == null ? "null" : "exist"));
//         Debug.Log(map);

//         EditorGUI.EndProperty();
//     }
// }

// [CustomPropertyDrawer(typeof(SlotBetting))]
// public class SlotBettingDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
//     {
//         // label = EditorGUI.BeginProperty(rect, label, property);
//         // Rect contentPos = EditorGUI.PrefixLabel(rect, label);

//         // int indent = EditorGUI.indentLevel;
//         // EditorGUI.indentLevel = 0;

//         // EditorGUIUtility.labelWidth = 40f;

//         // Rect wRect = new Rect(contentPos.x, contentPos.y, contentPos.width * 0.5f, contentPos.height);
//         // Rect hRect = new Rect(wRect.x + wRect.width, wRect.y, contentPos.width * 0.5f, contentPos.height);

//         // EditorGUI.PropertyField(wRect, property.FindPropertyRelative("distance"), new GUIContent("dis"));
//         // EditorGUI.PropertyField(hRect, property.FindPropertyRelative("duration"), new GUIContent("time"));

//         // EditorGUI.indentLevel = indent;

//         // EditorGUI.EndProperty();
//     }
// }