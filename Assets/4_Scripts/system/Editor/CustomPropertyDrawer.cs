using UnityEngine;
using UnityEditor;

using lpesign.UnityEditor;

[CustomPropertyDrawer(typeof(Size2D))]
public class Size2DDrawer : UsableDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Initialize();
        label = EditorGUI.BeginProperty(position, label, property);
        Rect contentPos = EditorGUI.PrefixLabel(position, label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        EditorGUIUtility.labelWidth = 15f;

        var propW = property.FindPropertyRelative("width");
        var propH = property.FindPropertyRelative("height");

        var labelW = new GUIContent("W");
        var labelH = new GUIContent("H");

        Rect wRect = new Rect(contentPos.x, contentPos.y, contentPos.width * 0.5f, _singleHeight);
        EditorGUI.PropertyField(wRect, propW, labelW);
        Rect hRect = new Rect(wRect.x + wRect.width, wRect.y, contentPos.width * 0.5f, _singleHeight);
        EditorGUI.PropertyField(hRect, propH, labelH);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(MoveTweenInfo))]
public class MoveTweenInfoDrawer : UsableDrawer
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