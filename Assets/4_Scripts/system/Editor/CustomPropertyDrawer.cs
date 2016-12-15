using UnityEngine;

using UnityEditor;
using DG.DOTweenEditor.Core;

using lpesign;

[CustomPropertyDrawer(typeof(Size2D))]
public class Size2DDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		label = EditorGUI.BeginProperty( position, label, property );
		Rect contentPos =  EditorGUI.PrefixLabel( position, label );

		int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;


		EditorGUIUtility.labelWidth = 15f;
		Rect wRect = new Rect( contentPos.x, contentPos.y, contentPos.width * 0.5f, contentPos.height);
		Rect hRect = new Rect( wRect.x + wRect.width, wRect.y, contentPos.width * 0.5f, contentPos.height);

		EditorGUI.PropertyField( wRect, property.FindPropertyRelative("width"),new GUIContent("W"));
		EditorGUI.PropertyField( hRect, property.FindPropertyRelative("height"),new GUIContent("H"));

		EditorGUI.indentLevel = indent;
		
		EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(MoveTweenInfo))]
public class MoveTweenInfoDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
		label = EditorGUI.BeginProperty( position, label, property );
		Rect contentPos =  EditorGUI.PrefixLabel( position, label );

		int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

		EditorGUIUtility.labelWidth = 40f;

		Rect wRect = new Rect( contentPos.x, contentPos.y, contentPos.width * 0.5f, contentPos.height);
		Rect hRect = new Rect( wRect.x + wRect.width, wRect.y, contentPos.width * 0.5f, contentPos.height);

		EditorGUI.PropertyField( wRect, property.FindPropertyRelative("distance"),new GUIContent("dis"));
		EditorGUI.PropertyField( hRect, property.FindPropertyRelative("duration"),new GUIContent("time"));

		EditorGUI.indentLevel = indent;
		
		EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer (typeof(SoundSchemaAttribute))]
public class SoundSchemaDrawer : PropertyDrawer
{
	SoundSchemaAttribute TargetAttribute{ get{ return attribute as SoundSchemaAttribute; }}
	// Draw the property inside the given rect
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
	{
		Debug.Log("--------------------------");
		SoundSchemaAttribute attr = TargetAttribute;

		SerializedObject obj = property.serializedObject;
		Debug.Log( obj );
		Debug.Log( obj is SoundSchema );
		Debug.Log( obj is SoundSchemaAttribute );

		SerializedProperty myFloat = obj.FindProperty( "myFloat" );
		SerializedProperty myschemaName = obj.FindProperty( "schemaName" );

		Debug.Log( myFloat.objectReferenceValue is SoundSchema );
		Debug.Log( myschemaName.objectReferenceValue is SoundSchema );

		Debug.Log( property.serializedObject.targetObject );//Test( ShowDecoratorDrawerExample );
		Debug.Log( myFloat );



		if( property.serializedObject.targetObject is  Component)
		{
			var component = property.serializedObject.targetObject as Component;
			Debug.Log("ok " + component );
		}
		else
		{
			Debug.Log("fail");
		}

		Debug.Log("--------------------------");
	}
}
