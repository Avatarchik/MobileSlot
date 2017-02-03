using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

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
            DrawMultiPropertyFields("host", "port");
            DrawMultiPropertyFields("accessID", "ver");

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
            EndFoldout();
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
            EndFoldout();
        }

        void DrawMachine(SerializedProperty machineProp, int index)
        {
            EditorGUILayout.BeginVertical(GUI.skin.button);

            DrawTab(index);
            DrawBase(machineProp);
            DrawReel(machineProp);
            DrawSymbol(machineProp, index);
            DrawScatterInfoList(machineProp);
            DrawSpin(machineProp);
            DrawTransition(machineProp);
            DrawPaylineTable(machineProp, index);
            DrawReelStrip(machineProp, index);

            EditorGUILayout.EndVertical();
        }

        void DrawTab(int index)
        {
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
        }

        void DrawBase(SerializedProperty machineProp)
        {
            var baseInfo = BeginFoldout(machineProp.FindPropertyRelative("row"), "Base");
            if (baseInfo.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawHorizontalProperties(machineProp, "row", "column");

                var freespin = DrawHorizontalProperties(machineProp, 75, "UseFreeSpin");
                if (freespin.boolValue) DrawHorizontalProperties(machineProp, 90, "TriggerType", "RetriggerType");
                EditorGUI.indentLevel--;
            }
            EndFoldout();
        }

        void DrawReel(SerializedProperty machineProp)
        {
            var reelInfo = BeginFoldout(machineProp.FindPropertyRelative("ReelSize"), "Reel");
            if (reelInfo.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawPropertyField(machineProp, "ReelSize");
                DrawHorizontalProperties(machineProp, 80, "ReelSpace", "ReelGap");

                var sliderLimit = machineProp.FindPropertyRelative("row").intValue;
                EditorGUILayout.IntSlider(machineProp.FindPropertyRelative("MarginSymbolCount"), 0, sliderLimit);

                DrawPropertyField(machineProp, 80, "ReelPrefab");
                EditorGUI.indentLevel--;
            }
            EndFoldout();
        }

        void DrawSymbol(SerializedProperty machineProp, int index)
        {
            //symbols
            var symbolInfo = BeginFoldout(machineProp.FindPropertyRelative("SymbolSize"), "Symbol");
            if (symbolInfo.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawPropertyField(machineProp, 70, "SymbolSize");
                var useEmpty = DrawPropertyField(machineProp, 80, "useEmpty").boolValue;
                if (useEmpty) DrawPropertyField(machineProp, 70, "BlankSymbolSize");
                EditorGUI.indentLevel--;

                DrawSymbolDefineList(machineProp, index);
                DrawStartSymbol(machineProp, index);
            }
            EndFoldout();
        }

        ReorderableList _symbolDefineList;

        void DrawSymbolDefineList(SerializedProperty machineProp, int index)
        {
            GUILayout.Space(5);
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

        void DrawStartSymbol(SerializedProperty machineProp, int index)
        {
            EditorGUILayout.LabelField("StartSymbols");
            EditorGUILayout.BeginVertical(GUI.skin.box);
            var machine = _script.GetMachineAt(index);
            var col = machine.column;
            var row = machine.row;
            var margin = machine.MarginSymbolCount;
            row += margin * 2;

            var symbolNames = machine.GetSymbolNames();

            for (var r = 0; r < row; ++r)
            {
                if (r == margin || r == row - margin) DrawRowLine(36 * col);

                EditorGUILayout.BeginHorizontal();
                for (var c = 0; c < col; ++c)
                {
                    var startSymbolNames = machine.GetStartSymbolNames(c);
                    var symbolIndex = symbolNames.IndexOf(startSymbolNames[r]);
                    var selectedIndex = EditorGUILayout.Popup(symbolIndex, symbolNames, GUILayout.Width(33));
                    startSymbolNames[r] = symbolNames[selectedIndex];

                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        void DrawScatterInfoList(SerializedProperty machineProp)
        {
            var scatters = BeginFoldout(machineProp.FindPropertyRelative("_scatters"), "ScatterInfo");
            if (scatters.isExpanded)
            {
                for (var i = 0; i < scatters.arraySize; ++i)
                {
                    var element = scatters.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginVertical(GUI.skin.box);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Type", GUILayout.Width(30));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("type"), GUIContent.none, GUILayout.Width(80));

                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("MinCount", GUILayout.Width(60));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("minCount"), GUIContent.none, GUILayout.Width(15));

                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("MaxCount", GUILayout.Width(60));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("maxCount"), GUIContent.none, GUILayout.Width(15));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();

                    var expectThreshold = element.FindPropertyRelative("expectThreshold");
                    EditorGUILayout.LabelField("expectThreshold", GUILayout.Width(95));
                    EditorGUILayout.PropertyField(expectThreshold, GUIContent.none, GUILayout.Width(15));
                    if (expectThreshold.intValue > 0)
                    {
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField("sound", GUILayout.Width(40));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("expectSound"), GUIContent.none, GUILayout.Width(130));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Reel", GUILayout.Width(30));
                    var re = element.FindPropertyRelative("ableReel");
                    DrawIntArrayBox(re, machineProp.FindPropertyRelative("column").intValue);
                    EditorGUILayout.EndHorizontal();

                    var sounds = BeginFoldout(element.FindPropertyRelative("stopSounds"));
                    if (sounds.isExpanded)
                    {
                        EditorGUILayout.BeginHorizontal();
                        DrawAudioClipArray(element.FindPropertyRelative("stopSounds"));
                        EditorGUILayout.EndHorizontal();
                    }
                    EndFoldout();


                    EditorGUILayout.EndVertical();
                }
            }
            EndFoldout();
        }

        void DrawSpin(SerializedProperty machineProp)
        {
            var spinInfo = BeginFoldout(machineProp.FindPropertyRelative("SpinSpeedPerSec"), "Spin");
            if (spinInfo.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawPropertyField(machineProp, "ReelSize");
                DrawPropertyField(machineProp, 140, "MarginSymbolCount");
                DrawPropertyField(machineProp, 140, "IncreaseCount");
                DrawPropertyField(machineProp, 140, "SpinningSymbolCount");
                DrawPropertyField(machineProp, 140, "SpinCountThreshold");
                DrawPropertyField(machineProp, 140, "DelayEachSpin");
                DrawPropertyField(machineProp, "tweenFirstBackInfo");
                DrawPropertyField(machineProp, "tweenLastBackInfo");
                EditorGUI.indentLevel--;
            }
            EndFoldout();
        }

        void DrawTransition(SerializedProperty machineProp)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Space(13);
            DrawPropertyField(machineProp, "transition");
            EditorGUILayout.EndHorizontal();
        }

        void DrawPaylineTable(SerializedProperty machineProp, int index)
        {
            var table = machineProp.FindPropertyRelative("paylineTable").FindPropertyRelative("_table");

            var paylineTable = BeginFoldout(machineProp.FindPropertyRelative("paylineTable"), "Payline  total: " + table.arraySize);
            if (paylineTable.isExpanded)
            {
                GUILayout.Space(5);
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Write JS Array Format", GUILayout.Width(250)))
                {
                    if (EditorUtility.DisplayDialog("Write JS Array Format?", "not yet implemented", "OK", "Cancel"))
                    {
                        //todo
                    }
                }
                GUI.backgroundColor = _defaultBGColor;
                GUILayout.Space(5);

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
            EndFoldout();
        }


        string _displayedStripsGroup = string.Empty;

        void DrawReelStrip(SerializedProperty machineProp, int index)
        {
            var bundleprop = machineProp.FindPropertyRelative("reelStripsBundle");
            var containsGroupProp = bundleprop.FindPropertyRelative("_containsGroup");

            var reelStripList = BeginFoldout(machineProp.FindPropertyRelative("reelStripsBundle"),
                                             "ReelStrips total: " + containsGroupProp.arraySize);
            if (reelStripList.isExpanded)
            {
                var enumType = typeof(ReelStripsBundle.Group);
                if (containsGroupProp.arraySize > 0)
                {
                    if (_displayedStripsGroup.IsEmpty())
                    {
                        _displayedStripsGroup = containsGroupProp.GetArrayElementAtIndex(0).GetEnumValue(enumType).ToString();
                    }

                    GUI.backgroundColor = Color.yellow;
                    if (GUILayout.Button("Load From File", GUILayout.Width(250)))
                    {
                        if (EditorUtility.DisplayDialog("Load From File?", "not yet implemented", "OK", "Cancel"))
                        {
                            //todo
                        }
                    }
                    GUI.backgroundColor = _defaultBGColor;

                    //draw contains Gruop tap
                    EditorGUILayout.BeginHorizontal();
                    for (var i = 0; i < containsGroupProp.arraySize; ++i)
                    {
                        var element = containsGroupProp.GetArrayElementAtIndex(i);
                        var enumName = element.GetEnumValue(enumType).ToString();

                        if (_displayedStripsGroup == enumName) GUI.backgroundColor = Color.cyan;
                        if (GUILayout.Button(enumName, GUILayout.Width(50)))
                        {
                            _displayedStripsGroup = enumName;
                        }
                        GUI.backgroundColor = _defaultBGColor;
                    }
                    EditorGUILayout.EndHorizontal();

                    //draw strips
                    var machine = _script.GetMachineAt(index);
                    var symbolAllNames = machine.GetSymbolNames();
                    ReelStripsBundle.Group chooseGroup = (ReelStripsBundle.Group)System.Enum.Parse(enumType, _displayedStripsGroup);
                    var reelStrips = machine.reelStripsBundle.GetStrips(chooseGroup);

                    EditorGUILayout.BeginHorizontal();
                    for (var c = 0; c < reelStrips.Length; ++c)
                    {
                        var rows = reelStrips.GetStripAt(c);
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        for (var r = 0; r < rows.Length; ++r)
                        {
                            var sname = rows[r];
                            var symbolIndex = symbolAllNames.IndexOf(sname);
                            var selectedIndex = EditorGUILayout.Popup(symbolIndex, symbolAllNames, GUILayout.Width(33));
                            rows[r] = symbolAllNames[selectedIndex];
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    //empty reelStrips
                }
            }
            EndFoldout();
        }
    }
}

