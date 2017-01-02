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
                DrawScatterInfoList(machineProp, index);

                //startSymbolNames
                DrawPropertyField(machineProp, "startSymbolNames");
            }
            EndFoldOut();

            //freespin
            EditorGUILayout.BeginVertical(GUI.skin.box);
            var freespin = DrawHorizontalProperties(machineProp, 75, "UseFreeSpin");
            if (freespin.boolValue)
            {
                DrawHorizontalProperties(machineProp, 90, "TriggerType", "RetriggerType");
                // DrawHorizontalProperties(element, 20, "TriggerType");
                // DrawHorizontalProperties(element, "RetriggerType");
            }
            EditorGUILayout.EndVertical();

            //reel
            var reelInfo = BeginFoldout(machineProp, "ReelSize", "Reel");
            if (reelInfo.isExpanded)
            {
                DrawPropertyField(machineProp, "ReelSize");
                DrawHorizontalProperties(machineProp, 80, "ReelSpace", "ReelGap");
                DrawPropertyField(machineProp, 80, "ReelPrefab");
            }
            EndFoldOut();

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

            //transitoin
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawPropertyField(machineProp, "transition");
            EditorGUILayout.EndVertical();

            //transitoin
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawPropertyField(machineProp, "paylineTable");
            EditorGUILayout.EndVertical();

            //reelStripBundle
            EditorGUILayout.BeginVertical(GUI.skin.box);
            DrawPropertyField(machineProp, "reelStripBundle");
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
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

        void DrawColumn(float w = 30)
        {
            EditorGUILayout.BeginHorizontal();
            string[] speedNames = new string[] { "0", "1", "2" };
            int[] speedValues = new int[] { 0, 1, 2 };
            var a = EditorGUILayout.IntPopup(0, speedNames, speedValues, GUILayout.Width(w));
            var b = EditorGUILayout.IntPopup(1, speedNames, speedValues, GUILayout.Width(w));
            var c = EditorGUILayout.IntPopup(2, speedNames, speedValues, GUILayout.Width(w));
            var d = EditorGUILayout.IntPopup(0, speedNames, speedValues, GUILayout.Width(w));
            var e = EditorGUILayout.IntPopup(1, speedNames, speedValues, GUILayout.Width(w));
            EditorGUILayout.EndHorizontal();
        }

        ReorderableList _scatterInfoList;
        void DrawScatterInfoList(SerializedProperty machineProp, int index)
        {
            var spinInfo = BeginFoldout(machineProp, "_scatters", "ScatterInfo");
            if (spinInfo.isExpanded)
            {
                var machine = _script.GetMachineAt(index);
                DrawColumn();


                EndFoldOut();
                // return;

                // //scatter
                // if (machine.ScatterInfos == null || machine.ScatterInfos.Count == 0)
                // {
                //     var symbolList = machine.SymbolDefineList;
                //     for (var i = 0; i < symbolList.Count; ++i)
                //     {
                //         var symbol = symbolList[i];
                //         if (SymbolDefine.IsScatter(symbol))
                //         {
                //             var info = new ScatterInfo(symbol.type);
                //             machine.AddScatterInfo(info);
                //         }
                //     }
                // }

                _scatterInfoList = CreateScatterInfoList(machineProp.FindPropertyRelative("_scatters"));
                _scatterInfoList.DoLayoutList();
            }


            var sct = machineProp.FindPropertyRelative("_scatters");
            EditorGUILayout.PropertyField(sct, true);
        }

        ReorderableList CreateScatterInfoList(SerializedProperty property)
        {
            if (_scatterInfoList != null) return _scatterInfoList;

            var list = CreateReorderableList(property, "", false);
            // list.elementHeight = EditorGUIUtility.singleLineHeight * 4f;

            list.drawElementCallback = (Rect position, int index, bool isActive, bool isFocused) =>
            {
                //type, limit, ableReel, stopSounds
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                Rect posName = new Rect(position.x, position.y, 80, _singleHeight);
                Rect posLimit = new Rect(posName.xMax, position.y, 20, _singleHeight);
                // Rect posSound = new Rect(posLimit.xMax + 50, position.y, 80, _singleHeight);
                // Rect posBuffer = new Rect(position.xMax - 30, position.y, 30, _singleHeight);

                EditorGUI.PropertyField(posName, element.FindPropertyRelative("type"), GUIContent.none);
                EditorGUI.PropertyField(posLimit, element.FindPropertyRelative("limit"), GUIContent.none);
                // EditorGUI.PropertyField(posSound, element.FindPropertyRelative("stopSounds"), GUIContent.none,true);
                // EditorGUI.PropertyField(posBuffer, element.FindPropertyRelative("buffer"), GUIContent.none);
            };

            return list;
        }

    }
}

