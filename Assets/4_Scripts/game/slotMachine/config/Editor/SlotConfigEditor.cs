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
            DrawBase();
            DrawBetting();
            DrawMachines();
            DrawMachinesBasic();

            Apply();
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

        void DrawBase()
        {
            EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);

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
            var betting = serializedObject.FindProperty("Betting");

            EditorGUILayout.LabelField("Betting", EditorStyles.boldLabel);

            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                DrawHorizontalProperties(betting, 100f, "_currentLineBet", "_lastLineBet");
                EditorGUILayout.EndVertical();
            }

            if (_bettingList == null)
            {
                var table = betting.FindPropertyRelative("_betTable");
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
        void DrawMachines()
        {
            var list = serializedObject.FindProperty("_machineList");
            EditorGUILayout.LabelField("Machines", EditorStyles.boldLabel);
            GUILayout.BeginVertical(GUI.skin.button);

            for (var i = 0; i < list.arraySize; ++i)
            {
                var element = list.GetArrayElementAtIndex(i);

                //nick
                if (i == 0) EditorGUILayout.LabelField(" Main", EditorStyles.boldLabel);
                else EditorGUILayout.LabelField(ConvertUtil.IntoToOrdinal(i + 1), EditorStyles.boldLabel);

                EditorGUILayout.BeginVertical(GUI.skin.button);

                //BASE
                var baseInfo = BeginFoldout(element, "Row", "Base");
                if (baseInfo.isExpanded)
                {
                    DrawHorizontalProperties(element, "Row", "Column");
                }
                EndFoldOut();

                //symbols
                var symbolInfo = BeginFoldout(element, "SymbolSize", "Symol");
                if (symbolInfo.isExpanded)
                {
                    DrawPropertyField(element, 70, "SymbolSize");
                    DrawPropertyField(element, 70, "NullSymbolSize");
                    EditorGUILayout.LabelField("Symbol's define & name mapping is here\nscatters and name maps etc..", GUILayout.Height(32));
                }
                EndFoldOut();

                //namemap
                // EditorGUILayout.LabelField("▼ NameMap", EditorStyles.miniLabel);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUI.indentLevel++;
                DrawPropertyField(element, "nameMap");
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }

        void DrawMachinesBasic()
        {
            var machines = serializedObject.FindProperty("_machineList");
            EditorGUILayout.PropertyField(machines, true);
        }
    }
}

