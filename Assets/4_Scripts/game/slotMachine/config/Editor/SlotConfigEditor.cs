using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using lpesign;
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

            if (_creator != null)
            {
                _script.name = _creator.gameObject.name;
            }
        }

        public override void OnInspectorGUI()
        {
            Update();

            DrawScript();
            DrawScriptSetting();
            DrawBasic();
            DrawBetting();
            DrawMachines();

            // var machines = serializedObject.FindProperty("_machineList");
            // EditorGUILayout.PropertyField(machines, true);

            Apply();
        }

        void DrawScriptSetting()
        {
            if (_creator == null) return;

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Set By Creator", GUILayout.Width(150), GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Set By Creator?", "Are you sure you want to set all list? This action cannot be undone.", "OK", "Cancel"))
                {
                    _creator.SettingByScript();
                }
            }
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = _defaultBGColor;
            EditorGUILayout.EndHorizontal();
        }

        void DrawBasic()
        {
            EditorGUILayout.LabelField("Basic", EditorStyles.boldLabel);

            GUILayout.BeginVertical(GUI.skin.button, GUILayout.ExpandWidth(true));

            DrawHorizontalProperties("name", "ID");
            DrawHorizontalProperties("Host", "Port");
            DrawHorizontalProperties("Version");
            DrawHorizontalProperties("Jackpot");

            GUILayout.BeginVertical(GUI.skin.button);
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            DrawHorizontalProperties(130f, "DebugSymbolArea", "DebugTestSpin");
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }


        ReorderableList _bettingList;

        void DrawBetting()
        {

            var bet = BeginFoldout("Betting");
            if (bet.isExpanded)
            {
                if (Application.isPlaying)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    DrawHorizontalProperties(bet, 100f, "_currentLineBet", "_lastLineBet");
                    EditorGUILayout.EndVertical();
                }

                if (_bettingList == null)
                {
                    var table = bet.FindPropertyRelative("_betTable");
                    _bettingList = CreateReorderableList(table, "", false);
                    _bettingList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        rect.y += 2;
                        rect.height -= 5F;
                        var element = _bettingList.serializedProperty.GetArrayElementAtIndex(index);
                        var GUIContent = new GUIContent("Bet " + (index + 1));
                        EditorGUI.PropertyField(rect, element, GUIContent);
                    };
                    _bettingList.onCanRemoveCallback = (ReorderableList l) =>
                    {
                        return l.count > 1;
                    };
                }

                _bettingList.DoLayoutList();
            }
            EndFoldOut();
        }

        void DrawMachines()
        {
            var bet = BeginFoldout("_machineList", "Machines");
            if (bet.isExpanded)
            {
                var list = serializedObject.FindProperty("_machineList");
                for (var i = 0; i < list.arraySize; ++i)
                {
                    var machine = list.GetArrayElementAtIndex(i);
                    DrawMachine(machine, i);
                }

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Add Machine", GUILayout.Height(30)))
                {
                    _script.AddMachine();
                }

                GUI.backgroundColor = _defaultBGColor;
            }
            EndFoldOut();
        }

        void DrawMachine(SerializedProperty machine, int index)
        {
            EditorGUI.indentLevel--;
            EditorGUILayout.BeginVertical(GUI.skin.button);

            EditorGUILayout.BeginHorizontal();

            //nick
            string nick = "";
            if (index == 0) nick = "Main";
            else nick = ConvertUtil.IntoToOrdinal(index + 1);
            EditorGUILayout.LabelField(nick, EditorStyles.boldLabel);

            //delete button
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X"))
            {
                if (EditorUtility.DisplayDialog("Delete Machine(" + nick + ")?", "Are you sure you want to delete " + nick + "? This action cannot be undone.", "Delete", "Cancel"))
                {
                    _script.RemoveMachineAt(index);
                }
            }
            GUI.backgroundColor = _defaultBGColor;

            EditorGUILayout.EndHorizontal();

            //BASE
            var baseInfo = BeginFoldout(machine, "Row", "Base");
            if (baseInfo.isExpanded)
            {
                DrawHorizontalProperties(machine, "Row", "Column");
            }
            EndFoldOut();

            //symbols
            var symbolInfo = BeginFoldout(machine, "SymbolSize", "Symbol");
            if (symbolInfo.isExpanded)
            {
                DrawPropertyField(machine, 70, "SymbolSize");
                DrawPropertyField(machine, 70, "NullSymbolSize");

                EditorGUILayout.LabelField("Scatters info...");
                //namemap
                DrawPropertyField(machine, "nameMap");

                //startSymbolNames
                DrawPropertyField(machine, "startSymbolNames");
            }
            EndFoldOut();

            //freespin
            EditorGUILayout.BeginVertical(GUI.skin.box);
            var freespin = DrawHorizontalProperties(machine, 75, "UseFreeSpin");
            if (freespin.boolValue)
            {
                DrawHorizontalProperties(machine, 90, "TriggerType", "RetriggerType");
                // DrawHorizontalProperties(element, 20, "TriggerType");
                // DrawHorizontalProperties(element, "RetriggerType");
            }
            EditorGUILayout.EndVertical();

            //reel
            var reelInfo = BeginFoldout(machine, "ReelSize", "Reel");
            if (reelInfo.isExpanded)
            {
                DrawPropertyField(machine, "ReelSize");
                DrawHorizontalProperties(machine, 80, "ReelSpace", "ReelGap");
                DrawPropertyField(machine, 80, "ReelPrefab");
            }
            EndFoldOut();

            //spin
            var spinInfo = BeginFoldout(machine, "SpinSpeedPerSec", "Spin");
            if (spinInfo.isExpanded)
            {
                DrawPropertyField(machine, "ReelSize");
                DrawPropertyField(machine, 140, "MarginSymbolCount");
                DrawPropertyField(machine, 140, "IncreaseCount");
                DrawPropertyField(machine, 140, "SpiningSymbolCount");
                DrawPropertyField(machine, 140, "SpinCountThreshold");
                DrawPropertyField(machine, 140, "DelayEachReel");
                DrawPropertyField(machine, "tweenFirstBackInfo");
                DrawPropertyField(machine, "tweenLastBackInfo");
            }
            EndFoldOut();

            //transitoin
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            DrawPropertyField(machine, "transition");
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            //transitoin
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            DrawPropertyField(machine, "paylineTable");
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            //reelStripBundle
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;
            DrawPropertyField(machine, "reelStripBundle");
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel++;
        }
    }
}

