using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hamsterbyte.PoolMaster;

[CustomEditor( typeof(PoolMasterSpawnPoints))]
public class PoolMasterSpawnPointsEditor : Editor {

	public Color _gray;
	public GUILayoutOption[] options = new GUILayoutOption[]{GUILayout.ExpandWidth (true)};
	public static List<bool> _spawnFoldouts = new List<bool> ();
	
	private PoolMasterSpawnPoints spawnPointsInstance;

	#region INSPECTOR
	public override void OnInspectorGUI () {
		CheckInstance();
		if (EditorGUIUtility.isProSkin) {
			_gray = Color.gray;
		} else {
			_gray = Color.white;
		}
		DrawInspector();
	}

	private void DrawInspector(){
		VerifyFoldouts();
		DrawInspectorHeader();
		DrawInspectorGroups();
	}

	private void DrawInspectorHeader() {
		GUI.backgroundColor = _gray;
		EditorGUILayout.BeginVertical ("button");
		GUI.backgroundColor = Color.magenta;
		EditorGUILayout.BeginHorizontal ("button", options);
		GUILayout.FlexibleSpace ();
		EditorGUILayout.LabelField ("POOL MASTER SPAWN POINTS",  EditorStyles.boldLabel, GUILayout.MaxWidth (200));
		GUILayout.FlexibleSpace ();
		EditorGUILayout.EndHorizontal ();
		GUI.backgroundColor = _gray;

		//SHOW HANDLES BLOCK
		EditorGUILayout.BeginVertical ("button", options);
		EditorGUILayout.BeginHorizontal ("button", options);
		EditorGUILayout.LabelField ("Handles");
		GUILayout.FlexibleSpace ();
		spawnPointsInstance.handleMode = EditorGUILayout.Popup (spawnPointsInstance.handleMode, spawnPointsInstance.HandleModes);
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();
		EditorGUILayout.EndVertical();

	}

	private void DrawInspectorGroups () {
		GUILayout.BeginVertical("button");
		for (int a = 0; a < spawnPointsInstance.spawnGroups.Count; a++) {
			EditorGUILayout.BeginVertical ("button", options);
			EditorGUILayout.BeginHorizontal ("miniLabel", options);
			if(!Application.isPlaying){
				GUI.backgroundColor = Color.red;
				if (GUILayout.Button ("X", GUILayout.Width (20))) {
					if (EditorUtility.DisplayDialog ("Delete " + spawnPointsInstance.spawnGroups [a].name + "?", "Are you sure you want to delete " + spawnPointsInstance.spawnGroups [a].name + "? This action cannot be undone.", "Delete Group", "Cancel")) {
						spawnPointsInstance.RemoveGroup(a);
						_spawnFoldouts.RemoveAt(a);
						return;
					}
				}
			}
			GUI.backgroundColor = _gray;
			EditorGUILayout.LabelField (spawnPointsInstance.spawnGroups [a].name, options);
			GUI.backgroundColor = Color.magenta;
			_spawnFoldouts [a] = EditorGUILayout.Toggle (_spawnFoldouts [a], "button", GUILayout.Width (20));
			GUI.backgroundColor = _gray;
			EditorGUILayout.EndHorizontal ();
			if (spawnPointsInstance.spawnGroups.FindAll (aName => aName.name == spawnPointsInstance.spawnGroups [a].name).Count > 1) {
				spawnPointsInstance.spawnGroups [a].name = spawnPointsInstance.spawnGroups [a].name + "X";
				EditorUtility.SetDirty (spawnPointsInstance);
			}
			if (_spawnFoldouts [a]) {
				if(spawnPointsInstance.handleMode == 1){
					EditorGUILayout.BeginHorizontal ();
					GUILayout.FlexibleSpace();
					EditorGUILayout.LabelField ("Show Handles", GUILayout.Width (90));
					spawnPointsInstance.spawnGroups [a].showHandles = EditorGUILayout.Toggle(spawnPointsInstance.spawnGroups [a].showHandles, GUILayout.Width (20));
					EditorGUILayout.EndHorizontal ();
				}
				spawnPointsInstance.spawnGroups[a].name = EditorGUILayout.TextField("Name", spawnPointsInstance.spawnGroups[a].name);
				GUI.backgroundColor = Color.white;
				spawnPointsInstance.spawnGroups[a].handleColor = EditorGUILayout.ColorField("Handle Color", spawnPointsInstance.spawnGroups[a].handleColor);
				GUI.backgroundColor = _gray;
				spawnPointsInstance.spawnGroups[a].handleShape = EditorGUILayout.Popup("Handle Shape", spawnPointsInstance.spawnGroups[a].handleShape, spawnPointsInstance.spawnGroups[a].HandleShapes);
				if(spawnPointsInstance.spawnGroups[a].spawnPoints != null){
					GUILayout.BeginVertical(EditorStyles.textArea);
					for(int s = 0; s < spawnPointsInstance.spawnGroups[a].spawnPoints.Count; s++){
						GUILayout.BeginHorizontal();
						//DELETE BUTTON
						if(!Application.isPlaying){
							GUI.backgroundColor = Color.red;
							if (GUILayout.Button ("X", GUILayout.Width (20))) {
								spawnPointsInstance.spawnGroups[a].RemovePoint(s);
								return;
							}
							GUI.backgroundColor = _gray;
						}
						//VECTOR FIELD
						spawnPointsInstance.spawnGroups[a].spawnPoints[s] = EditorGUILayout.Vector3Field("", spawnPointsInstance.spawnGroups[a].spawnPoints[s]);
						GUILayout.EndHorizontal();
						GUILayout.Space(5);
					}
					GUILayout.EndVertical();
				} else {
					spawnPointsInstance.spawnGroups[a].spawnPoints = new List<Vector3>();
				}

				DrawAddPointButton(a);
			}


			EditorGUILayout.EndVertical();

		}
		GUILayout.Space(10);
		DrawAddGroupButton();
		EditorGUILayout.EndVertical ();
		DrawClearAllGroupsButton();
		if(GUI.changed)
			EditorUtility.SetDirty(spawnPointsInstance);
	}

	private void DrawClearAllGroupsButton(){
		GUI.backgroundColor = Color.red;
		EditorGUILayout.BeginVertical ("button", options);
		GUILayout.Space (5);
		GUI.backgroundColor = _gray;
		if (GUILayout.Button ("Clear All Groups", options)) {
			if (EditorUtility.DisplayDialog ("Clear All Spawn Groups?", "Are you sure you want to clear all spawn groups? This action cannot be undone.", "Clear All", "Cancel")) {
				_spawnFoldouts = new List<bool> ();
				spawnPointsInstance.spawnGroups = new List<SpawnGroup> ();
			}
		}
		GUI.backgroundColor = _gray;
		GUILayout.Space (5);
		EditorGUILayout.EndVertical ();
	}
	private void DrawAddGroupButton(){
		GUI.backgroundColor = Color.green;
		if (GUILayout.Button ("Add Spawn Group", options)) {
			_spawnFoldouts.Add (false);
			spawnPointsInstance.AddGroup("New Spawn Group");
			if (spawnPointsInstance.spawnGroups.FindAll (group => group.name.Contains ("New Spawn Group")).Count > 1 && spawnPointsInstance.spawnGroups != null) {
				spawnPointsInstance.spawnGroups [spawnPointsInstance.spawnGroups.Count - 1].name = "New Spawn Group " + (spawnPointsInstance.spawnGroups.FindAll (group => group.name.Contains ("New Spawn Group")).Count).ToString ();
				EditorUtility.SetDirty (spawnPointsInstance);
			}
			return;
		}
		GUI.backgroundColor = _gray;
	}

	private void DrawAddPointButton (int index) {
		GUI.backgroundColor = Color.green;
		if (GUILayout.Button ("Add Spawn Point", options)) {
			spawnPointsInstance.spawnGroups[index].AddPoint();
			return;
		}
		GUI.backgroundColor = _gray;
	}

	private void VerifyFoldouts(){
		if(_spawnFoldouts != null) {
			if(_spawnFoldouts.Count < spawnPointsInstance.spawnGroups.Count){
				int diff = spawnPointsInstance.spawnGroups.Count - _spawnFoldouts.Count;
				for(int i = 0; i < diff; i++){
					_spawnFoldouts.Add(false);
				}
			}
			return;
		} else {
			_spawnFoldouts = new List<bool>();
			return;
		}
	}
	#endregion


	#region SCENE
	private void OnScene (SceneView sceneView) {

		CheckInstance();
		VerifyFoldouts();
		if(spawnPointsInstance.handleMode > 0){
			for(int i = 0; i < spawnPointsInstance.spawnGroups.Count; i++){
				DrawSpawnPoints(i);
			}
		}
	}

	private void OnEnable(){
		SceneView.onSceneGUIDelegate += OnScene;
	}

	private void OnDisable() {
		SceneView.onSceneGUIDelegate -= OnScene;
	}
	
	private void CheckInstance(){
		if(spawnPointsInstance == null)
			spawnPointsInstance = (PoolMasterSpawnPoints)target;
	}
	
	private void DrawSpawnPoints(int i){
		if(spawnPointsInstance.spawnGroups[i] != null){
			Handles.color = spawnPointsInstance.spawnGroups[i].handleColor;
			for(int p = 0; p < spawnPointsInstance.spawnGroups[i].spawnPoints.Count; p++){
				if(spawnPointsInstance.handleMode == 1){
					if(spawnPointsInstance.spawnGroups[i].showHandles) {
						if(_spawnFoldouts[i]){
							if(!Application.isPlaying)
								spawnPointsInstance.spawnGroups[i].spawnPoints[p] = Handles.DoPositionHandle(spawnPointsInstance.spawnGroups[i].spawnPoints[p], Quaternion.identity);
						}
						Handles.Label(spawnPointsInstance.spawnGroups[i].spawnPoints[p] + Vector3.right * .25f, spawnPointsInstance.spawnGroups[i].name + "(" + p.ToString() + ")");
						if(spawnPointsInstance.spawnGroups[i].handleShape == 0){
							Handles.SphereCap(i, spawnPointsInstance.spawnGroups[i].spawnPoints[p], Quaternion.identity, .5f);
						}
						if(spawnPointsInstance.spawnGroups[i].handleShape == 1){
							Handles.CubeCap(i, spawnPointsInstance.spawnGroups[i].spawnPoints[p], Quaternion.identity, .5f);
						}
						if(spawnPointsInstance.spawnGroups[i].handleShape == 2){
							Handles.ConeCap(i, spawnPointsInstance.spawnGroups[i].spawnPoints[p] + Vector3.up * .25f, Quaternion.Euler(90, 0, 0), .5f);
						}
						if(spawnPointsInstance.spawnGroups[i].handleShape == 3){
							Handles.CubeCap(i, spawnPointsInstance.spawnGroups[i].spawnPoints[p], Quaternion.Euler(0, 0, 45), .5f);
						}
					}
				} else {
					if(_spawnFoldouts[i]){
						if(!Application.isPlaying)
							spawnPointsInstance.spawnGroups[i].spawnPoints[p] = Handles.DoPositionHandle(spawnPointsInstance.spawnGroups[i].spawnPoints[p], Quaternion.identity);
					}
					Handles.Label(spawnPointsInstance.spawnGroups[i].spawnPoints[p] + Vector3.right * .25f, spawnPointsInstance.spawnGroups[i].name + "(" + p.ToString() + ")");
					if(spawnPointsInstance.spawnGroups[i].handleShape == 0){
						Handles.SphereCap(i, spawnPointsInstance.spawnGroups[i].spawnPoints[p], Quaternion.identity, .5f);
					}
					if(spawnPointsInstance.spawnGroups[i].handleShape == 1){
						Handles.CubeCap(i, spawnPointsInstance.spawnGroups[i].spawnPoints[p], Quaternion.identity, .5f);
					}
					if(spawnPointsInstance.spawnGroups[i].handleShape == 2){
						Handles.ConeCap(i, spawnPointsInstance.spawnGroups[i].spawnPoints[p] + Vector3.up * .25f, Quaternion.Euler(90, 0, 0), .5f);
					}
					if(spawnPointsInstance.spawnGroups[i].handleShape == 3){
						Handles.CubeCap(i, spawnPointsInstance.spawnGroups[i].spawnPoints[p], Quaternion.Euler(0, 0, 45), .5f);
					}
				}
			}
		}
		HandleUtility.Repaint();
	}
	#endregion

}