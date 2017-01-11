using UnityEngine;
using UnityEditor;

namespace lpesign.UnityEditor
{
    [CustomPropertyDrawer(typeof(DrawableDictionary), true)]
    public class DictionaryPropertyDrawer : UsableDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                var keysProp = property.FindPropertyRelative("_keys");
                var valueHeight = (keysProp.arraySize + 2) * EditorGUIUtility.singleLineHeight;
                var buttonHeight = 20;
                return valueHeight + buttonHeight;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize();

            label = EditorGUI.BeginProperty(position, label, property);

            bool expanded = property.isExpanded;
            var rect = GetNextRect(ref position);
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label);
            if (expanded)
            {
                var keyList = property.FindPropertyRelative("_keys");
                var valueList = property.FindPropertyRelative("_values");

                int keyCount = keyList.arraySize;
                if (valueList.arraySize != keyCount) valueList.arraySize = keyCount;


                //display key & value
                for (int i = 0; i < keyCount; i++)
                {
                    rect = GetNextRect(ref position);
                    rect = EditorGUI.IndentedRect(rect);

                    var w = rect.width / 2f;
                    var keyRect = new Rect(rect.xMin, rect.yMin, w, rect.height);
                    var valueRect = new Rect(keyRect.xMax, rect.yMin, w, rect.height);

                    var keyProp = keyList.GetArrayElementAtIndex(i);
                    var valueProp = valueList.GetArrayElementAtIndex(i);
                    // GUIContent.none
                    EditorGUIUtility.labelWidth = 60;
                    var keyLabel = new GUIContent("key");
                    var valueLabel = new GUIContent("value");

                    EditorGUI.PropertyField(keyRect, keyProp, keyLabel);
                    EditorGUI.PropertyField(valueRect, valueProp, valueLabel,false);
                }

                //+ - button
                rect = GetNextRect(ref position);
                rect.y += 3;

                var pRect = new Rect(rect.xMax - 60f, rect.yMin, 30f, EditorGUIUtility.singleLineHeight);
                var mRect = new Rect(rect.xMax - 30f, rect.yMin, 30f, EditorGUIUtility.singleLineHeight);

                GUI.backgroundColor = Color.green;
                if (GUI.Button(pRect, "+"))
                {
                    keyList.arraySize++;
                    var element = keyList.GetArrayElementAtIndex(keyList.arraySize - 1);
                    element.SetPropertyValue(null);
                    valueList.arraySize = keyList.arraySize;
                }
                GUI.backgroundColor = _defaultBGColor;

                GUI.backgroundColor = Color.red;
                if (GUI.Button(mRect, "-"))
                {
                    keyList.arraySize = Mathf.Max(keyList.arraySize - 1, 0);
                    valueList.arraySize = keyList.arraySize;
                }
                GUI.backgroundColor = _defaultBGColor;
            }

            EditorGUI.EndProperty();
        }
    }
}
