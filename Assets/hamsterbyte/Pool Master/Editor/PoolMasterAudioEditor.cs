/*
**************************************
************ POOL MASTER *************
**************************************
______________________________________

VERSION: 3.0
FILE:    POOLMASTERAUDIOEDITOR.CS
AUTHOR:  CODY JOHNSON
COMPANY: HAMSTERBYTE, LLC
EMAIL:   HAMSTERBYTELLC@GMAIL.COM
WEBSITE: WWW.HAMSTERBYTE.COM
SUPPORT: WWW.HAMSTERBYTE.COM/POOL-MASTER

COPYRIGHT © 2014 HAMSTERBYTE, LLC
ALL RIGHTS RESERVED
*/

using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using hamsterbyte.PoolMaster;

[System.Serializable]
[CustomEditor(typeof(PoolMasterAudio))]
public class PoolMasterAudioEditor : Editor {

	public PoolMasterAudio targetInstance;
	public Color _gray;
	public GUILayoutOption[] options = new GUILayoutOption[]{GUILayout.ExpandWidth (true)};
	public static List<bool> _audioFoldouts = new List<bool> ();

	public override void OnInspectorGUI () {
		targetInstance = (PoolMasterAudio)target;
		if (EditorGUIUtility.isProSkin) {
			_gray = Color.gray;
		} else {
			_gray = Color.white;
		}

		int totalClips = 0;

		for(int clipCount = 0; clipCount < targetInstance.audioClips.Count; clipCount++)
			totalClips++;

		if(targetInstance.activeClips > targetInstance.audioBuffer)
			targetInstance.audioBuffer = targetInstance.activeClips;

		GUI.backgroundColor = _gray;
		EditorGUILayout.BeginVertical ("button");
		GUI.backgroundColor = Color.yellow;
		EditorGUILayout.BeginHorizontal ("button", options);
		GUILayout.FlexibleSpace ();
		EditorGUILayout.LabelField ("POOL MASTER AUDIO",  EditorStyles.boldLabel, GUILayout.MaxWidth (140));
		GUILayout.FlexibleSpace ();
		EditorGUILayout.EndHorizontal ();
		GUI.backgroundColor = _gray;
		EditorGUILayout.BeginVertical ("button", options);
		EditorGUILayout.BeginHorizontal ("button", options);
		EditorGUILayout.LabelField ("Total Clips: " + totalClips.ToString(), GUILayout.MaxWidth (100));
		GUILayout.FlexibleSpace ();
		if(Application.isPlaying){
			EditorGUILayout.LabelField (targetInstance.activeClips + "/" + targetInstance.audioBuffer, GUILayout.MaxWidth (50));
			if(targetInstance.activeClips == 0)
				GUI.backgroundColor = Color.red;
			else
				GUI.backgroundColor = Color.green;
			EditorGUILayout.LabelField (" ", EditorStyles.radioButton, GUILayout.Width (20));
			GUI.backgroundColor = _gray;
		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical();
		if(!Application.isPlaying){
			GUILayout.Space(5);
			targetInstance.audioBuffer = EditorGUILayout.IntSlider("Audio Buffer", targetInstance.audioBuffer, 1, 255, options);
			GUILayout.Space(5);
			targetInstance.audioMixer = (AudioMixer)EditorGUILayout.ObjectField ("Audio Mixer", targetInstance.audioMixer, typeof(AudioMixer), true, options);

		}

		EditorGUILayout.EndVertical ();

		if (targetInstance.audioClips != null) {
			if (Event.current.type == EventType.Layout)
			{

				if (_audioFoldouts.Count < targetInstance.audioClips.Count) {
					int dif = targetInstance.audioClips.Count - _audioFoldouts.Count;
					for (int i = 0; i < dif; i++) {
						_audioFoldouts.Add (false);
					}
				}
				
				if(targetInstance.clipLimit != null){
					if (targetInstance.clipLimit.Count < targetInstance.audioClips.Count) {
						int dif = targetInstance.audioClips.Count - targetInstance.clipLimit.Count;
						for (int i = 0; i < dif; i++) {
							targetInstance.clipLimit.Add (targetInstance.audioBuffer);
						}
						return;
					}
				} else {
					targetInstance.clipLimit = new List<int>();
					return;
				}

				if(targetInstance.mixerGroupID != null){
					if (targetInstance.mixerGroupID.Count < targetInstance.audioClips.Count) {
						int dif = targetInstance.audioClips.Count - targetInstance.mixerGroupID.Count;
						for (int i = 0; i < dif; i++) {
							targetInstance.mixerGroupID.Add (0);
						}
						return;
					}
				} else {
					targetInstance.mixerGroupID = new List<int>();
					return;
				}
			}


			EditorGUILayout.BeginVertical ("button");
			for (int a = 0; a < targetInstance.audioClips.Count; a++) {
					EditorGUILayout.BeginVertical ("button", options);
					EditorGUILayout.BeginHorizontal ("miniLabel", options);
					if(!Application.isPlaying){
						GUI.backgroundColor = Color.red;
						if (GUILayout.Button ("X", GUILayout.Width (20))) {
							targetInstance.audioClips.RemoveAt (a);
							targetInstance.audioNames.RemoveAt (a);
							targetInstance.audioVolumes.RemoveAt (a);
							targetInstance.audioPans.RemoveAt (a);
							targetInstance.audioPriorities.RemoveAt (a);
							targetInstance.audioPitches.RemoveAt (a);
							targetInstance.activeCount.RemoveAt (a);
							targetInstance.clipLimit.RemoveAt(a);
							targetInstance.mixerGroupID.RemoveAt(a);
							_audioFoldouts.RemoveAt(a);
							return;
						}
					}
					GUI.backgroundColor = _gray;
					EditorGUILayout.LabelField (targetInstance.audioNames [a], options);
					if(Application.isPlaying){
						if(targetInstance.activeCount [a] == 0)
							GUI.backgroundColor = Color.red;
						else
						GUI.backgroundColor = Color.green;
						EditorGUILayout.LabelField (" ", EditorStyles.radioButton, GUILayout.Width (20));
					}
					
					if(!Application.isPlaying){
						GUI.backgroundColor = Color.yellow;
						_audioFoldouts [a] = EditorGUILayout.Toggle (_audioFoldouts [a], "button", GUILayout.Width (20));
					}
					GUI.backgroundColor = _gray;
					EditorGUILayout.EndHorizontal ();
					if (targetInstance.audioNames.FindAll (aName => aName == targetInstance.audioNames [a]).Count > 1) {
						targetInstance.audioNames [a] = targetInstance.audioNames [a] + "X";
						EditorUtility.SetDirty (targetInstance);
					}
					if (_audioFoldouts [a] && targetInstance.activeCount[a] == 0) {
						if(targetInstance.audioMixer != null){
							targetInstance.mixerGroupArray = targetInstance.audioMixer.FindMatchingGroups(string.Empty);
							List<string> tList = new List<string>();
							foreach(AudioMixerGroup aGroup in targetInstance.mixerGroupArray)
								tList.Add(aGroup.name);
							targetInstance.mixerGroupNames = tList.ToArray();
						}
						EditorGUILayout.BeginHorizontal ();
						GUILayout.FlexibleSpace();
						EditorGUILayout.LabelField ("Persistent", GUILayout.Width (70));
						targetInstance.audioPersistence [a] = EditorGUILayout.Toggle(targetInstance.audioPersistence [a], GUILayout.Width (20));
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Name", GUILayout.Width (50));
						targetInstance.audioNames [a] = EditorGUILayout.TextField (targetInstance.audioNames [a], options);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Clip", GUILayout.Width (50));
						targetInstance.audioClips [a] = (AudioClip)EditorGUILayout.ObjectField (targetInstance.audioClips [a], typeof(AudioClip), true, options);
						EditorGUILayout.EndHorizontal ();
						if(targetInstance.audioMixer != null){
							EditorGUILayout.BeginHorizontal ();
							targetInstance.mixerGroupID[a] = EditorGUILayout.Popup("Mixer Group", targetInstance.mixerGroupID[a], targetInstance.mixerGroupNames); 
							EditorGUILayout.EndHorizontal ();
						}
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Limit", GUILayout.Width (50));
						targetInstance.clipLimit [a] = EditorGUILayout.IntSlider (targetInstance.clipLimit [a], 0, 64, options);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Volume", GUILayout.Width (50));
						targetInstance.audioVolumes [a] =  (float)Math.Round(EditorGUILayout.Slider (targetInstance.audioVolumes [a], 0, 1, options), 2);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Pan", GUILayout.Width (50));
						targetInstance.audioPans [a] = EditorGUILayout.Slider (targetInstance.audioPans [a], -1, 1, options);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Pitch", GUILayout.Width (50));
						targetInstance.audioPitches [a] = EditorGUILayout.Slider (targetInstance.audioPitches [a], -3, 3, options);
						EditorGUILayout.EndHorizontal ();
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Priority", GUILayout.Width (50));
						targetInstance.audioPriorities [a] = EditorGUILayout.IntSlider (targetInstance.audioPriorities [a], 0, 255, options);
						EditorGUILayout.EndHorizontal ();
				}
					EditorGUILayout.EndVertical ();
			}
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button ("Add Clip", options)) {
				_audioFoldouts.Add (false);
				targetInstance.audioClips.Add (null);
				targetInstance.audioVolumes.Add (1);
				targetInstance.audioPans.Add (0);
				targetInstance.audioPriorities.Add (128);
				targetInstance.audioPitches.Add (1);
				targetInstance.activeCount.Add(0);
				targetInstance.audioPersistence.Add(false);
				targetInstance.clipLimit.Add(targetInstance.audioBuffer);
				targetInstance.audioNames.Add ("New Audio Clip");
				targetInstance.mixerGroupID.Add(0);
				if (targetInstance.audioNames.FindAll (aName => aName.Contains ("New Audio Clip")).Count > 1 && targetInstance.audioClips != null) {
					targetInstance.audioNames [targetInstance.audioNames.Count - 1] = "New Audio Clip " + (targetInstance.audioNames.FindAll (aName => aName.Contains ("New Audio Clip")).Count).ToString ();
					EditorUtility.SetDirty (targetInstance);
				}
			}
			GUI.backgroundColor = _gray;
			EditorGUILayout.EndVertical ();
		}
		if (targetInstance.audioClips != null) {
		
			if (targetInstance.audioClips.Count > 0) {
				GUI.backgroundColor = Color.red;
				EditorGUILayout.BeginVertical ("button", options);
				GUILayout.Space (5);
				GUI.backgroundColor = _gray;
				if (GUILayout.Button ("Clear All Clips", options)) {
					if (EditorUtility.DisplayDialog ("Clear All Audio Clips?", "Are you sure you want to clear all audio clips? This action cannot be undone.", "Clear All", "Cancel")) {
						targetInstance.audioClips = new List<AudioClip> ();
						targetInstance.audioNames = new List<string> ();
						targetInstance.audioVolumes = new List<float> ();
						targetInstance.audioPans = new List<float> ();
						targetInstance.audioPitches = new List<float> ();
						targetInstance.audioPriorities = new List<int> ();
						targetInstance.activeCount = new List<int>();
						targetInstance.audioPersistence = new List<bool>();
						targetInstance.clipLimit = new List<int>();
						targetInstance.mixerGroupID = new List<int>();
						_audioFoldouts = new List<bool>();
					}
				}
				GUI.backgroundColor = _gray;
				GUILayout.Space (5);
				EditorGUILayout.EndVertical ();
			}
		}

		
		if (GUI.changed)
			EditorUtility.SetDirty (targetInstance);
		
	}
}
