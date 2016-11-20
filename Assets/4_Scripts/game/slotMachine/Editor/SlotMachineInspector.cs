using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

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

            var stateNames = from s in _state orderby s ascending select s.ToString();
            foreach (string n in stateNames)
            {
                if (_state.Peek().ToString() == n)
                {
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField(n, EditorStyles.boldLabel);
                }
                else
                {
                    GUI.color = Color.gray;
                    EditorGUILayout.LabelField( "  " + n);
                }
            }

            EditorGUILayout.EndVertical();

            GUI.color = Color.white;
        }

    }
}