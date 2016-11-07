/*
 * FEATURE UNDER CONSTRUCTION
 * 
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace hamsterbyte.PoolMaster {
	public class PoolMasterWindow : EditorWindow {

		#region VARS
		private GUILayoutOption[] left;
		private GUILayoutOption[] right;
		private GUILayoutOption stretch = GUILayout.ExpandWidth(true);
		private int _curTab;
		private Color Gray{get{return(EditorGUIUtility.isProSkin)? Color.gray : Color.white;}}
		#endregion

		[MenuItem ("Window/Pool Master")]
		public static void  ShowWindow () {
			EditorWindow.GetWindow(typeof(PoolMasterWindow), false, "Pool Master", true);
		}
		
		void OnGUI(){
			DrawHeader();
			DrawTab(_curTab);
			if(GUI.changed){
				this.Repaint();
			}
		}

		private void DrawHeader(){
			GUI.backgroundColor = Gray;
			GUILayout.BeginHorizontal(EditorStyles.toolbar, stretch);
			EditorGUILayout.LabelField("Pool Master v3.1", GUILayout.Width(130));
			GUILayout.FlexibleSpace();
			GUI.backgroundColor = (_curTab == 0)? Color.green : Color.clear;
			if(GUILayout.Button("Pools", EditorStyles.toolbarButton, GUILayout.Width(60))){
				_curTab = 0;
			}
			GUI.backgroundColor = (_curTab == 1)? Color.green : Color.clear;
			if(GUILayout.Button("Audio", EditorStyles.toolbarButton, GUILayout.Width(60))){
				_curTab = 1;
			}
			GUI.backgroundColor = (_curTab == 2)? Color.green : Color.clear;
			if(GUILayout.Button("Spawn", EditorStyles.toolbarButton, GUILayout.Width(60))){
				_curTab = 2;
			}
			GUILayout.EndHorizontal();
		}

		private void DrawTab(int i){
			if(i == 0)
				DrawPoolsTab();
			if(i == 1)
				DrawAudioTab ();
			if(i == 2)
				DrawSpawnTab();
		}

		private void DrawPoolsTab(){
			if(Application.isPlaying){
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("Not available in play mode!");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			} else {
				GUI.backgroundColor = Gray;
				GUILayout.BeginHorizontal(EditorStyles.textField, stretch);
				EditorGUILayout.LabelField("Pools", EditorStyles.boldLabel, GUILayout.Width(position.width * .25f));
				EditorGUILayout.LabelField("Objects", EditorStyles.boldLabel, GUILayout.Width(position.width * .75f));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical(left);
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("Not available in play mode!");
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.BeginVertical(right);
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("Not available in play mode!");
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				GUI.backgroundColor = Gray;
				GUILayout.BeginHorizontal(EditorStyles.toolbar, stretch);
				if(GUILayout.Button("Add Pool", EditorStyles.toolbarButton, GUILayout.Width(75))){
					//ADD POOL CODE
				}
				if(GUILayout.Button("Clear Pools", EditorStyles.toolbarButton, GUILayout.Width(75))){
					//CLEAR POOLS CODE
				}
				GUILayout.EndHorizontal();
			}
		}

		private void DrawAudioTab(){
			if(Application.isPlaying){
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("Not available in play mode!");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			}
		}

		private void DrawSpawnTab(){
			if(Application.isPlaying){
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField("Not available in play mode!");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			}
		}

		private void SetupPositions(){
			left = new GUILayoutOption[]{GUILayout.Width(position.width * .25f), GUILayout.Height(position.height - 30)};
			right = new GUILayoutOption[]{GUILayout.Width(position.width * .75f), GUILayout.Height(position.height - 30)};
		}
	}
}
*/


