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

            DrawSetByCreatorButton();
            DrawBasic();
            DrawBetting();
            DrawMachines();

            // DrawColumn();

            // var machines = serializedObject.FindProperty("_machineList");
            // EditorGUILayout.PropertyField(machines, true);

            Apply();
        }

        void DrawSetByCreatorButton()
        {
            if (_creator == null) return;

            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Set By Creator", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Set By Creator?", "Are you sure you want to set all list? This action cannot be undone.", "OK", "Cancel"))
                {
                    _creator.SettingByScript();
                }
            }
            GUI.backgroundColor = _defaultBGColor;
        }

        void DrawBasic()
        {
            EditorGUILayout.LabelField("Basic", EditorStyles.boldLabel);

            GUILayout.BeginVertical(GUI.skin.button, GUILayout.ExpandWidth(true));

            DrawMultiPropertyFields("name", "ID");
            DrawMultiPropertyFields("Host", "Port");
            DrawPropertyField("Version");
            DrawPropertyField("Jackpot");

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

        void DrawMachine(SerializedProperty machineProp, int index)
        {
            EditorGUILayout.BeginVertical(GUI.skin.button);

            EditorGUILayout.BeginHorizontal();

            //nick
            string nick = "";
            if (index == 0) nick = "Main";
            else nick = ConvertUtil.IntToOrdinal(index + 1);
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
            var baseInfo = BeginFoldout(machineProp, "row", "Base");
            if (baseInfo.isExpanded)
            {
                DrawHorizontalProperties(machineProp, "row", "column");

                var freespin = DrawHorizontalProperties(machineProp, 75, "UseFreeSpin");
                if (freespin.boolValue) DrawHorizontalProperties(machineProp, 90, "TriggerType", "RetriggerType");
            }
            EndFoldOut();

            //reel
            var reelInfo = BeginFoldout(machineProp, "ReelSize", "Reel");
            if (reelInfo.isExpanded)
            {
                DrawPropertyField(machineProp, "ReelSize");
                DrawHorizontalProperties(machineProp, 80, "ReelSpace", "ReelGap");

                var sliderLimit = machineProp.FindPropertyRelative("row").intValue;
                EditorGUILayout.IntSlider(machineProp.FindPropertyRelative("MarginSymbolCount"), 0, sliderLimit);

                DrawPropertyField(machineProp, 80, "ReelPrefab");
            }
            EndFoldOut();

            //symbols
            var symbolInfo = BeginFoldout(machineProp, "SymbolSize", "Symbol");
            if (symbolInfo.isExpanded)
            {
                DrawPropertyField(machineProp, 70, "SymbolSize");
                var useEmpty = DrawPropertyField(machineProp, 80, "useEmpty").boolValue;
                if (useEmpty) DrawPropertyField(machineProp, 70, "NullSymbolSize");

                DrawSymbolDefineList(machineProp, index);
                DrawStartSymbol(machineProp, index);
            }
            EndFoldOut();

            DrawScatterInfoList(machineProp, index);


            //spin
            var spinInfo = BeginFoldout(machineProp, "SpinSpeedPerSec", "Spin");
            if (spinInfo.isExpanded)
            {
                DrawPropertyField(machineProp, "ReelSize");
                DrawPropertyField(machineProp, 140, "MarginSymbolCount");
                DrawPropertyField(machineProp, 140, "IncreaseCount");
                DrawPropertyField(machineProp, 140, "SpiningSymbolCount");
                DrawPropertyField(machineProp, 140, "SpinCountThreshold");
                DrawPropertyField(machineProp, 140, "DelayEachReel");
                DrawPropertyField(machineProp, "tweenFirstBackInfo");
                DrawPropertyField(machineProp, "tweenLastBackInfo");
            }
            EndFoldOut();


            DrawTransition(machineProp);
            DrawPaylineTable(machineProp, index);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawPropertyField(machineProp, "reelStripBundle");
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        void DrawStartSymbol(SerializedProperty machineProp, int index)
        {
            EditorGUILayout.LabelField("StartSymbols");
            EditorGUILayout.BeginVertical(GUI.skin.box);
            var machine = _script.GetMachineAt(index);
            var col = machine.column;
            var row = machine.row;
            var margin = machine.MarginSymbolCount;
            row += margin * 2;

            var startSymbolSet = machineProp.FindPropertyRelative("_startSymbolSet");
            var names = machine.GetSymbolNames();

            for (var r = 0; r < row; ++r)
            {
                if (r == margin || r == row - margin) DrawRowLine(33 * col);

                EditorGUILayout.BeginHorizontal();
                for (var c = 0; c < col; ++c)
                {
                    var symbolName = machine.GetStartSymbolAt(c, r);
                    var symbolIndex = names.IndexOf(symbolName);
                    var selectedIndex = EditorGUILayout.Popup(symbolIndex, names, GUILayout.Width(30));
                    machine.SetStartSymbolAt(c, r, names[selectedIndex]);

                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawTransition(SerializedProperty machineProp)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Space(15);
            DrawPropertyField(machineProp, "transition");
            EditorGUILayout.EndHorizontal();
        }

        void DrawPaylineTable(SerializedProperty machineProp, int index)
        {
            var paylineTable = BeginFoldout(machineProp, "paylineTable", "Payline");
            if (paylineTable.isExpanded)
            {
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Write JS Array Format"))
                {
                    if (EditorUtility.DisplayDialog("Write JS Array Format?", "not yet implemented", "OK", "Cancel"))
                    {
                        //todo
                    }
                }
                if (GUILayout.Button("Load From File"))
                {
                    if (EditorUtility.DisplayDialog("Load From File?", "not yet implemented", "OK", "Cancel"))
                    {
                        //todo
                    }
                }
                GUI.backgroundColor = _defaultBGColor;
                EditorGUILayout.EndHorizontal();

                var table = paylineTable.FindPropertyRelative("_table");
                EditorGUILayout.LabelField("Total: " + table.arraySize, EditorStyles.boldLabel);

                for (var i = 0; i < table.arraySize; ++i)
                {
                    var element = table.GetArrayElementAtIndex(i);
                    var arr = element.FindPropertyRelative("rows");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("line " + i, GUILayout.Width(50));
                    DrawIntArrayBox(arr, machineProp.FindPropertyRelative("column").intValue);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EndFoldOut();
        }

        ReorderableList _symbolDefineList;

        void DrawSymbolDefineList(SerializedProperty machineProp, int index)
        {
            GUILayout.Space(5);
            var machine = _script.GetMachineAt(index);
            _symbolDefineList = CreateSymbolDefineList(machineProp.FindPropertyRelative("_symbolDefineList"));
            _symbolDefineList.DoLayoutList();
        }

        ReorderableList CreateSymbolDefineList(SerializedProperty property)
        {
            if (_symbolDefineList != null) return _symbolDefineList;

            var list = CreateReorderableList(property, "SymbolDefine", false);
            list.drawElementCallback = (Rect position, int index, bool isActive, bool isFocused) =>
            {
                //symbolName, type, prefab, buffer
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                Rect posName = new Rect(position.x, position.y, 30, _singleHeight);
                Rect posPrefab = new Rect(posName.xMax, position.y, position.xMax - posName.xMax - 80 - 30 - 5, _singleHeight);
                Rect posType = new Rect(position.xMax - 80 - 30, position.y, 80, _singleHeight);
                Rect posBuffer = new Rect(position.xMax - 30, position.y, 30, _singleHeight);

                EditorGUI.PropertyField(posName, element.FindPropertyRelative("symbolName"), GUIContent.none);
                EditorGUI.PropertyField(posPrefab, element.FindPropertyRelative("prefab"), GUIContent.none);
                EditorGUI.PropertyField(posType, element.FindPropertyRelative("type"), GUIContent.none);
                EditorGUI.PropertyField(posBuffer, element.FindPropertyRelative("buffer"), GUIContent.none);
            };

            return list;
        }

        void DrawScatterInfoList(SerializedProperty machineProp, int index)
        {
            var scatters = BeginFoldout(machineProp, "_scatters", "ScatterInfo");
            if (scatters.isExpanded)
            {
                var machine = _script.GetMachineAt(index);
                for (var i = 0; i < scatters.arraySize; ++i)
                {
                    var element = scatters.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("type"), GUIContent.none, GUILayout.Width(80));

                    EditorGUILayout.LabelField("Reel", EditorStyles.boldLabel, GUILayout.Width(30));
                    var re = element.FindPropertyRelative("ableReel");
                    DrawIntArrayBox(re, machineProp.FindPropertyRelative("column").intValue);

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("limit", EditorStyles.boldLabel, GUILayout.Width(32));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("limit"), GUIContent.none, GUILayout.Width(15));

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    var sounds = BeginFoldout(element, "stopSounds", "", "");
                    if (sounds.isExpanded)
                    {
                        EditorGUILayout.BeginHorizontal();
                        DrawAudioClipArray(element.FindPropertyRelative("stopSounds"));
                        EditorGUILayout.EndHorizontal();
                    }
                    EndFoldOut();

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                }
            }
            EndFoldOut();
        }
    }
}

