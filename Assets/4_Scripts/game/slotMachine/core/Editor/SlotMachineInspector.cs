using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using Game;

[CustomEditor(typeof(SlotMachine))]
public class SlotMachineInspector : Editor
{
    SlotMachine _machine;
    MonoScript _script;

    Stack<SlotMachine.MachineState> _state;

    void OnEnable()
    {
        _machine = target as SlotMachine;
        _script = MonoScript.FromMonoBehaviour(_machine);

        _state = _machine.State;
    }

    void DrawRowLine(string lineName, float height = 1f)
    {
        EditorGUILayout.Space();
        GUI.color = Color.white;
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(height) });
        GUILayout.Label(lineName, EditorStyles.boldLabel);
    }

    public override void OnInspectorGUI()
    {
        if (Application.isPlaying == false || _machine.gameObject.activeInHierarchy == false)
        {
            DrawDefaultInspector();
            return;
        }

        GUI.enabled = false;
        _script = EditorGUILayout.ObjectField("Script", _script, typeof(MonoScript), false) as MonoScript;
        GUI.enabled = true;

        DrawRowLine("State Log");

        if (_state.Count > 0)
        {
            EditorGUILayout.BeginVertical();

            var stateNames = _state.Select(s => s.ToString()).ToArray();
            System.Array.Reverse(stateNames);

            for (var i = 0; i < stateNames.Length; ++i)
            {
                var name = stateNames[i];
                if (i == stateNames.Length - 1)
                {
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
                }
                else
                {
                    GUI.color = Color.gray;
                    EditorGUILayout.LabelField("  " + name);
                }
            }

            EditorGUILayout.EndVertical();

            GUI.color = Color.white;
        }

    }
}