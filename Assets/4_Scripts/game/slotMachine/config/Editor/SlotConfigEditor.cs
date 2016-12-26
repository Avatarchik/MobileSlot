using UnityEngine;
using UnityEditor;
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

            if( _creator != null )
            {
                _script.name = _creator.gameObject.name;
            }
        }

        public override void OnInspectorGUI()
        {
            DrawScript();
            DrawScriptSetting();

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
    }
}

