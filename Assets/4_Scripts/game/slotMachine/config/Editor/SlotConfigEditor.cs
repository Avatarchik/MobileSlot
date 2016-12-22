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
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("SettingByScript", GUILayout.Height(40)))
            {
                _creator.SettingByScript();
            }
            GUI.backgroundColor = _defaultBGColor;
        }
    }
}

