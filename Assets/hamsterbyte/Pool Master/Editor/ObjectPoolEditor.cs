/*

**************************************
************ POOL MASTER *************
**************************************
______________________________________

VERSION: 3.0
FILE:    OBJECTPOOLEDITOR.CS
AUTHOR:  CODY JOHNSON
COMPANY: HAMSTERBYTE, LLC
EMAIL:   HAMSTERBYTELLC@GMAIL.COM
WEBSITE: WWW.HAMSTERBYTE.COM
SUPPORT: WWW.HAMSTERBYTE.COM/POOL-MASTER

COPYRIGHT © 2014-2015 HAMSTERBYTE, LLC
ALL RIGHTS RESERVED

*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using hamsterbyte.PoolMaster;

[System.Serializable]
[CustomEditor(typeof(ObjectPool))]

public class ObjectPoolEditor : Editor {
	//Version info variables
	private static string version = "3.0";
	private static string unityVersion = "4.6";
	public static List<bool> foldouts = new List<bool> ();
	public static bool showOptions = false;
	public ObjectPool oPool;
	private Color _gray;
	private bool _isPlaying;
	
	[MenuItem ("GameObject/Pool Master/New Pool Master", false, 0)]
	public static void CreateObjectPool () {
		if (Object.FindObjectOfType<ObjectPool> () == null) {
			GameObject gObj = new GameObject ("POOL MASTER");
			gObj.AddComponent<ObjectPool> ();
			gObj.AddComponent<PoolMasterAudio> ();
			gObj.AddComponent<PoolMasterSpawnPoints> ();
		} else {
			EditorUtility.DisplayDialog ("Cannot Create New Pool Master", "Pool Master already exists in this scene! You need only one.", "Okay");
		}
	}
	
	[MenuItem ("GameObject/Pool Master/Version Info", false, 50)]
	public static void VersionInfo () {
		EditorUtility.DisplayDialog ("Pool Master - Version Info", 
		                               "Version: " + version + System.Environment.NewLine +
			"Works With Unity: " + unityVersion + "+" + System.Environment.NewLine +
			"Copyright© 2014-2015 Hamsterbyte, LLC" + System.Environment.NewLine +
			"All Rights Reserved.", "Ok", "");
	}
	
	[MenuItem ("GameObject/Pool Master/Documentation", false, 51)]
	public static void LaunchSupport () {
		Application.OpenURL ("www.hamsterbyte.com/pool-master");
	}
	
	[MenuItem ("GameObject/Pool Master/Developer Website", false, 52)]
	public static void LaunchDeveloperWebsite () {
		Application.OpenURL ("www.hamsterbyte.com");
	}
	
	[MenuItem ("GameObject/Pool Master/Rate This Asset", false, 52)]
	public static void LaunchRatingLink () {
		Application.OpenURL ("http://u3d.as/aMm");
	}

	private void OnSceneGUI(){
		//hide transform handles as they are not needed and create confusion with spawn points
		Tools.current = Tool.None;
	}

	public override void OnInspectorGUI () {
		GUILayoutOption[] options = { GUILayout.ExpandWidth (true)};
		oPool = (ObjectPool)target;
		if (foldouts.Count < oPool.pools.Count) {
			int dif = oPool.pools.Count - foldouts.Count;
			for (int i = 0; i < dif; i++)
				foldouts.Add (false);
		}

		if (EditorGUIUtility.isProSkin) {
			_gray = Color.gray;
		} else {
			_gray = Color.white;
		}
		

		EditorGUILayout.BeginVertical ();
		int totalUnique = 0;
		int totalObjects = 0;
		if (oPool != null) {
			if(Application.isPlaying){
				foreach(Pool p in oPool.pools){
					totalObjects += p.pool.Count;
				}
				for (int i = 0; i < oPool.pools.Count; i++) {
					for (int g = 0; g < oPool.pools[i].uniquePool.Count; g++) {
						totalUnique++;
					}
					
				}
			} else {
				for (int i = 0; i < oPool.pools.Count; i++) {
					foreach (int b in oPool.pools[i].bufferAmount) {
						totalObjects += b;
					}
					for (int g = 0; g < oPool.pools[i].uniquePool.Count; g++) {
						totalUnique++;
					}
					
				}
			}
		}

		GUI.backgroundColor = _gray;
		EditorGUILayout.BeginVertical ("button", options);
		GUI.backgroundColor = new Color (0, .66f, .66f);
		EditorGUILayout.BeginHorizontal ("button", options);
		GUILayout.FlexibleSpace ();
		EditorGUILayout.LabelField ("POOL MASTER v" + version,(EditorGUIUtility.isProSkin)? EditorStyles.boldLabel : EditorStyles.whiteBoldLabel, GUILayout.MaxWidth (130));
		GUILayout.FlexibleSpace ();
		EditorGUILayout.EndHorizontal ();
		GUI.backgroundColor = _gray;
		EditorGUILayout.BeginVertical ("button", options);
		EditorGUILayout.BeginHorizontal ("button", options);
		EditorGUILayout.LabelField ("Pools: " + oPool.pools.Count.ToString (), EditorStyles.miniLabel, GUILayout.MaxWidth (60));
		GUILayout.FlexibleSpace ();
		EditorGUILayout.LabelField ("Objects: " + totalUnique, EditorStyles.miniLabel, GUILayout.MaxWidth (70));
		GUILayout.FlexibleSpace ();
		if (oPool.smartBufferMode == 0) {
			if(!Application.isPlaying)
				EditorGUILayout.LabelField ("Total: " + totalObjects, EditorStyles.miniLabel, GUILayout.MaxWidth (100));
			else
				EditorGUILayout.LabelField ("Active: " + oPool.totalActive.ToString() + "/" + totalObjects, EditorStyles.miniLabel, GUILayout.MaxWidth (100));
		} else {
			if(!Application.isPlaying)
				EditorGUILayout.LabelField ("Total: " + totalObjects + "+", EditorStyles.miniLabel, GUILayout.MaxWidth (100));
			else
				EditorGUILayout.LabelField ("Active: " + oPool.totalActive.ToString() + "/" + totalObjects, EditorStyles.miniLabel, GUILayout.MaxWidth (100));
		}
		if(Application.isPlaying){
			if (oPool.totalActive <= 0)
				GUI.backgroundColor = Color.red;
			else
				GUI.backgroundColor = Color.green;
			EditorGUILayout.LabelField (" ", EditorStyles.radioButton, GUILayout.Width (20));
			GUI.backgroundColor = _gray;
		}
		EditorGUILayout.EndHorizontal ();
			
		//BEGIN SHOW OPTIONS
		if (!Application.isPlaying) {
			EditorGUILayout.BeginVertical ("button", options);
			EditorGUILayout.BeginHorizontal (options);
			EditorGUILayout.LabelField ("Show Options");
			GUILayout.FlexibleSpace ();
			GUI.backgroundColor = Color.green;
			showOptions = EditorGUILayout.Toggle (showOptions, "button", new GUILayoutOption[] {
										GUILayout.MaxWidth (20),
										GUILayout.MinWidth (20)
								}); 
			GUI.backgroundColor = _gray;
			EditorGUILayout.EndHorizontal ();
			if (showOptions) {
					
				//PRELOAD BLOCK
				EditorGUILayout.BeginVertical ("button", options);
				EditorGUILayout.BeginHorizontal ("button", options);
				EditorGUILayout.LabelField ("Preload");
				GUILayout.FlexibleSpace ();
				oPool.preloadMode = EditorGUILayout.Popup (oPool.preloadMode, oPool.PreloadModes);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
				GUILayout.Space (5);
					
					
				//PERSISTENCE BLOCK
				EditorGUILayout.BeginVertical ("button", options);
				EditorGUILayout.BeginHorizontal ("button", options);
				EditorGUILayout.LabelField ("Persistence");
				GUILayout.FlexibleSpace ();
				oPool.persistenceMode = EditorGUILayout.Popup (oPool.persistenceMode, oPool.PersistenceModes);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
				GUILayout.Space (5);
				
				//SMART BUFFERING BLOCK
				EditorGUILayout.BeginVertical ("button", options);
				EditorGUILayout.BeginHorizontal ("button", options);
				EditorGUILayout.LabelField ("Smart Buffering");
				GUILayout.FlexibleSpace ();
				oPool.smartBufferMode = EditorGUILayout.Popup (oPool.smartBufferMode, oPool.SmartBufferModes);
				EditorGUILayout.EndHorizontal ();
				if (oPool.smartBufferMode > 0) {
					EditorGUILayout.BeginHorizontal ("button", options);
					oPool.smartBufferMax = EditorGUILayout.IntField ("Max: ", Mathf.Clamp (oPool.smartBufferMax, 0, int.MaxValue));
					EditorGUILayout.EndHorizontal ();
					GUILayout.Space (5);	
				}
				EditorGUILayout.EndVertical ();
				GUILayout.Space (5);
					
				//EVENTS BLOCK
				EditorGUILayout.BeginVertical ("button", options);
				EditorGUILayout.BeginHorizontal ("button", options);
				EditorGUILayout.LabelField ("Events");
				GUILayout.FlexibleSpace ();
				oPool.eventMode = EditorGUILayout.Popup (oPool.eventMode, oPool.EventModes);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
				GUILayout.Space (5);
					
				//HIDE IN HIERARCHY BLOCK
				EditorGUILayout.BeginVertical ("button", options);
				EditorGUILayout.BeginHorizontal ("button", options);
				EditorGUILayout.LabelField ("Hide In Hierarchy");
				GUILayout.FlexibleSpace ();
				oPool.hideMode = EditorGUILayout.Popup (oPool.hideMode, oPool.HideModes);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
				GUILayout.Space (5);
				
					
				//DEBUGGING BLOCK
				EditorGUILayout.BeginVertical ("button", options);
				EditorGUILayout.BeginHorizontal ("button", options);
				EditorGUILayout.LabelField ("Debugging");
				GUILayout.FlexibleSpace ();
				oPool.debugMode = EditorGUILayout.Popup (oPool.debugMode, oPool.DebugModes);
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
				GUILayout.Space (5);
			}
			EditorGUILayout.EndVertical ();
		}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical ("button");
		if (oPool.pools.Count > 0) {
			for (int i = 0; i < oPool.pools.Count; i++) {				
				EditorGUILayout.BeginVertical ("button");
				GUILayout.BeginHorizontal (options);
				if (!Application.isPlaying) {
					GUI.backgroundColor = Color.red;
					if (GUILayout.Button ("X", GUILayout.Width (20))) {
						if (EditorUtility.DisplayDialog ("Delete " + oPool.pools [i].name + "?", "Are you sure you want to delete " + oPool.pools [i].name + "? This action cannot be undone.", "Delete Pool", "Cancel")) {
							oPool.pools.RemoveAt (i);
						}
						
					}
				}
				if (i < oPool.pools.Count) {
					if(Application.isPlaying){
						if (oPool.pools[i].totalActive <= 0)
							GUI.backgroundColor = Color.red;
						else
							GUI.backgroundColor = Color.green;
						EditorGUILayout.LabelField (" ", EditorStyles.radioButton, GUILayout.Width (20));
					}
					GUI.backgroundColor = _gray;
					EditorGUILayout.LabelField (oPool.pools [i].name, options);
					if(Application.isPlaying)
						EditorGUILayout.LabelField(oPool.pools[i].totalActive + "/" + oPool.pools [i].pool.Count, EditorStyles.miniBoldLabel, GUILayout.Width(80));
					GUI.backgroundColor = new Color (0, .66f, .66f);
					foldouts [i] = EditorGUILayout.Toggle (foldouts [i], "button", new GUILayoutOption[] {
												GUILayout.MaxWidth (20),
												GUILayout.MinWidth (20)
										});
					GUI.backgroundColor = _gray;
				}
				GUILayout.EndHorizontal ();
				if (foldouts [i]) {
					EditorGUILayout.BeginHorizontal ("box", options);
					if (i < oPool.pools.Count) {
						if (oPool.pools [i].uniquePool != null) {
							if (oPool.pools [i].uniquePool.Count != 0) {
								int totalItems = oPool.pools [i].bufferAmount.Take (oPool.pools [i].bufferAmount.Count).Sum ();
								EditorGUILayout.LabelField ("Unique: " + oPool.pools [i].uniquePool.Count.ToString (), GUILayout.MaxWidth (100));
								EditorGUILayout.LabelField ("Total: " + totalItems.ToString (), GUILayout.MaxWidth (100));
								
							} else {
								EditorGUILayout.LabelField ("Unique: 0", GUILayout.MaxWidth (100));
								EditorGUILayout.LabelField ("Total: 0", GUILayout.MaxWidth (100));
							}
							EditorGUILayout.EndHorizontal ();
							if (!Application.isPlaying) {
								EditorGUILayout.BeginVertical ("box", options);
								if (oPool.pools.FindAll (a => a.name == oPool.pools [i].name).Count > 1) {
									oPool.pools [i].name = oPool.pools [i].name + "X";
									EditorUtility.SetDirty (oPool);
								}
								if (oPool.pools [i].name == "ADVANCED AUDIO") {
									EditorUtility.DisplayDialog ("Name Reserved!", "'ADVANCED AUDIO' is a reserved name. Use something else.", "OK");
									oPool.pools [i].name = "Audio";
									EditorUtility.SetDirty (oPool);
								}
								if (oPool.persistenceMode == 1) {
									GUILayout.BeginHorizontal ();
									GUILayout.FlexibleSpace ();
									oPool.pools [i].persistent = EditorGUILayout.Toggle ("Persistent", oPool.pools [i].persistent, options);
									GUILayout.EndHorizontal ();
									GUILayout.Space (5);
								}
								oPool.pools [i].name = EditorGUILayout.TextField ("Pool Name: ", oPool.pools [i].name, options);
								oPool.pools [i].maxBuffer = Mathf.Clamp (EditorGUILayout.IntField ("Local Buffer: ", oPool.pools [i].maxBuffer, options), 1, int.MaxValue);
								if (oPool.preloadMode == 1)
									oPool.pools [i].preload = EditorGUILayout.Toggle ("Preload", oPool.pools [i].preload, options);
								if (oPool.hideMode == 1)
									oPool.pools [i].hide = EditorGUILayout.Toggle ("Hide In Hierarchy", oPool.pools [i].hide, options);
								GUILayout.Space (5);
								EditorGUILayout.EndVertical ();
							}
							
							
							if (oPool.pools [i].uniquePool.Count != 0) {
								EditorGUILayout.BeginHorizontal ("textfield");
								if (!Application.isPlaying) {
									GUI.backgroundColor = Color.black;
									GUILayout.Space (10);
									EditorGUILayout.LabelField ("Clamp", GUILayout.Width (40));
									EditorGUILayout.LabelField ("Buffer", GUILayout.Width (40));
									EditorGUILayout.LabelField ("Object", options);
									GUI.backgroundColor = _gray;
								} else {
									GUI.backgroundColor = Color.black;
									EditorGUILayout.LabelField ("Active", GUILayout.Width (60));
									EditorGUILayout.LabelField ("Object", options);
									GUI.backgroundColor = _gray;
								}
								EditorGUILayout.EndHorizontal ();	
							}
							EditorGUILayout.BeginVertical ("textfield");
							for (int g = 0; g < oPool.pools[i].uniquePool.Count; g++) {
								EditorGUILayout.BeginHorizontal ("box", options);
								if (!Application.isPlaying) {
									GUI.backgroundColor = Color.red;
									if (GUILayout.Button ("X", GUILayout.Width (20))) {
										oPool.pools [i].uniquePool.RemoveAt (g);
										oPool.pools [i].bufferAmount.RemoveAt (g);
										oPool.pools [i].alwaysMax.RemoveAt (g);
									}
									GUI.backgroundColor = _gray;
								}
								if (g < oPool.pools [i].uniquePool.Count) {
									GUILayout.Space (7);
									if (g < oPool.pools [i].uniquePool.Count) {
									
										//RESOLVES UPGRADE ERRORS////////////////////////////////////////////
										if (oPool.pools [i].alwaysMax.Count < oPool.pools [i].uniquePool.Count)
											oPool.pools [i].alwaysMax.Add (true);
										////////////////////////////////////////////////////////////////////////
										if (oPool.pools [i].alwaysMax.Count == oPool.pools [i].uniquePool.Count) {
											if (!Application.isPlaying)
												oPool.pools [i].alwaysMax [g] = EditorGUILayout.Toggle (oPool.pools [i].alwaysMax [g], GUILayout.Width (15));
											if (!Application.isPlaying) {
												if (!oPool.pools [i].alwaysMax [g]) {
													oPool.pools [i].bufferAmount [g] = EditorGUILayout.IntField (oPool.pools [i].bufferAmount [g], GUILayout.Width (40));
												} else {
													oPool.pools [i].bufferAmount [g] = Mathf.Clamp (EditorGUILayout.IntField (oPool.pools [i].bufferAmount [g], GUILayout.Width (40)), oPool.pools [i].maxBuffer, oPool.pools [i].maxBuffer);
												}
											} else {
												EditorGUILayout.LabelField (oPool.pools [i].activeCount [g].ToString () + "/" + oPool.pools [i].bufferAmount [g], EditorStyles.miniTextField, GUILayout.Width (80));
											}
										}
										if (!Application.isPlaying){
											oPool.pools [i].uniquePool [g] = (GameObject)EditorGUILayout.ObjectField (oPool.pools [i].uniquePool [g], typeof(GameObject), true, options);
										}else{
											if (oPool.pools [i].uniquePool [g] != null) {
												EditorGUILayout.LabelField (oPool.pools [i].uniquePool [g].name, options);
											} else {
												EditorGUILayout.LabelField ("Object is NULL", options);
											}
											if (oPool.pools [i].activeCount != null) {
												if (oPool.pools [i].activeCount [g] == 0)
													GUI.backgroundColor = Color.red;
												else
													GUI.backgroundColor = Color.green;
												EditorGUILayout.LabelField (" ", EditorStyles.radioButton, GUILayout.Width (20));
												GUI.backgroundColor = _gray;
											}
										}
									}
									
								}
								EditorGUILayout.EndHorizontal ();

							}

							if (!Application.isPlaying) {
								GUI.backgroundColor = Color.green;
								if (GUILayout.Button ("Add Object", options)) {
									oPool.pools [i].uniquePool.Add (null);
									oPool.pools [i].bufferAmount.Add (oPool.pools [i].maxBuffer);
									oPool.pools [i].alwaysMax.Add (true);
								}
							}
							GUILayout.Space (3);
							GUI.backgroundColor = _gray;
							EditorGUILayout.EndVertical ();
							
							
						}
					}
					if (i < oPool.pools.Count) {
						if (oPool.pools [i].uniquePool.Count != 0) {
							if (!Application.isPlaying) {
								GUI.backgroundColor = Color.red;
								if (GUILayout.Button ("Clear Pool", "button", options)) {
									if (EditorUtility.DisplayDialog ("Clear All Objects From " + oPool.pools [i].name + "?", "Are you sure you want to clear all objects from " + oPool.pools [i].name + "? This action cannot be undone.", "Clear Objects", "Cancel")) {
										oPool.pools [i].uniquePool = new List<GameObject> ();
										oPool.pools [i].bufferAmount = new List<int> ();
										oPool.pools [i].alwaysMax = new List<bool> ();
									}
								}
								GUILayout.Space (3);
								GUI.backgroundColor = _gray;
							}
						}
					}
				}

				EditorGUILayout.EndVertical ();
			}
		}
		if (!Application.isPlaying) {
			GUI.backgroundColor = Color.green;
			GUILayout.Space (3);
			if (GUILayout.Button ("Add Pool", options)) {
				foldouts.Add (false);
				oPool.pools.Add (new Pool ("New Pool"));
				if (oPool.pools.FindAll (a => a.name.Contains ("New Pool")).Count > 1) {
					oPool.pools [oPool.pools.Count - 1].name = "New Pool " + (oPool.pools.FindAll (a => a.name.Contains ("New Pool")).Count).ToString ();
					EditorUtility.SetDirty (oPool);
				}
			}
		}
		GUI.backgroundColor = _gray;
		EditorGUILayout.EndVertical ();
		if (!Application.isPlaying) {

			if (oPool.pools.Count > 0) {
				GUI.backgroundColor = Color.red;
				EditorGUILayout.BeginVertical ("button", options);
				GUILayout.Space (5);
				GUI.backgroundColor = _gray;
				if (GUILayout.Button ("Clear All Pools", options)) {
					if (EditorUtility.DisplayDialog ("Clear All Pools?", "Are you sure you want to clear all pools? This action cannot be undone.", "Clear All", "Cancel")) {
						foldouts = new List<bool> ();
						oPool.pools = new List<Pool> ();
					}
				}
				GUI.backgroundColor = _gray;
				GUILayout.Space (5);
				EditorGUILayout.EndVertical ();
			}

			
		}
		EditorGUILayout.EndVertical ();
		if (GUI.changed)
			EditorUtility.SetDirty (oPool);
		
	}	
}