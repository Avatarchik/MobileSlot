/*

**************************************
************ POOL MASTER *************
**************************************
______________________________________

VERSION: 3.0
FILE:    POOLMASTERAUDIO.CS
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
using System.Collections;
using System.Collections.Generic;
namespace hamsterbyte.PoolMaster {
	[RequireComponent(typeof(ObjectPool))]
	[System.Serializable]
	public class PoolMasterAudio : MonoBehaviour {

		public int audioBuffer = 50;
		public AudioMixer audioMixer;
		public string[] mixerGroupNames;
		public AudioMixerGroup[] mixerGroupArray;
		public List<int> mixerGroupID;
		public List<AudioClip> audioClips;
		public List<string> audioNames;
		public List<float> audioVolumes;
		public List<float> audioPans;
		public List<int> audioPriorities; 
		public List<float> audioPitches;
		public List<int> activeCount;
		public List<int> clipLimit;
		public int activeClips;
		public List<bool> audioPersistence;
		private ObjectPool objectPool;

		public PoolMasterAudio () {
			mixerGroupID = new List<int>();
			audioClips = new List<AudioClip>();
			audioNames = new List<string>();
			audioVolumes = new List<float>();
			audioPans = new List<float>();
			audioPriorities = new List<int>(); 
			audioPitches = new List<float>();
			activeCount = new List<int>();
			audioPersistence = new List<bool>();
			clipLimit = new List<int>();
		}

		void Awake(){
			objectPool = this.GetComponent<ObjectPool>();
		}

		public void PlayAudio (GameObject obj, string clipName) {
			if (audioNames.Contains (clipName)) {
				if (obj != null) {
					int index = audioNames.IndexOf (clipName);
					if(activeCount[index] < clipLimit[index]){
						PooledAudio pAudio = obj.GetComponent<PooledAudio> ();
						if (pAudio != null) {
							pAudio.Define (audioClips [index], audioVolumes [index], audioPans [index], audioPitches[index], audioPriorities [index], false, audioPersistence[index]);
							try {
								pAudio.SetMixerGroup(mixerGroupArray[mixerGroupID[index]]);
							} catch {
								pAudio.SetMixerGroup(mixerGroupArray[0]);
							}
							pAudio.Play ();
							if(objectPool.eventMode > 0)
								PoolMasterEvents.EventPlayAudio(clipName, obj);
						}
					}
				}
			} else {
				Debug.LogWarning ("PlayAudio: Clip '" + clipName + "' does not exist!");
			}
		}

		public void PlayAudio (GameObject obj, string clipName, bool loop) {
			if (audioNames.Contains (clipName)) {
				if (obj != null) {
					int index = audioNames.IndexOf (clipName);
					if(activeCount[index] < clipLimit[index]){
						PooledAudio pAudio = obj.GetComponent<PooledAudio> ();
						if (pAudio != null) {
							pAudio.Define (audioClips [index], audioVolumes [index], audioPans [index], audioPitches[index], audioPriorities [index], loop, audioPersistence[index]);
							try {
								pAudio.SetMixerGroup(mixerGroupArray[mixerGroupID[index]]);
							} catch {
								pAudio.SetMixerGroup(mixerGroupArray[0]);
							}
							pAudio.Play ();
							if(objectPool.eventMode > 0)
								PoolMasterEvents.EventPlayAudio(clipName, obj);
						}
					}
				}
			} else {
				Debug.LogWarning ("PlayAudio: Clip '" + clipName + "' does not exist!");
			}
		}

		public void PlayAudio (GameObject obj, int index) {
			if (obj != null) {
				int i = Mathf.Clamp(index, 0, audioNames.Count);
				if(activeCount[index] < clipLimit[index]){
					PooledAudio pAudio = obj.GetComponent<PooledAudio> ();
					if (pAudio != null) {
						pAudio.Define (audioClips [i], audioVolumes [i], audioPans [i], audioPitches[i], audioPriorities [i], false, audioPersistence[index]);
						try {
							pAudio.SetMixerGroup(mixerGroupArray[mixerGroupID[index]]);
						} catch {
							pAudio.SetMixerGroup(mixerGroupArray[0]);
						}
						pAudio.Play ();
						if(objectPool.eventMode > 0)
							PoolMasterEvents.EventPlayAudio(audioNames[i], obj);
					}
				}
			}

		}
		
		public void PlayAudio (GameObject obj, int index, bool loop) {
				if (obj != null) {
					int i = Mathf.Clamp(index, 0, audioNames.Count);
					if(activeCount[index] < clipLimit[index]){
						PooledAudio pAudio = obj.GetComponent<PooledAudio> ();
						if (pAudio != null) {
							pAudio.Define (audioClips [i], audioVolumes [i], audioPans [i], audioPitches[i], audioPriorities [i], loop, audioPersistence[index]);
							try {
								pAudio.SetMixerGroup(mixerGroupArray[mixerGroupID[index]]);
							} catch {
								pAudio.SetMixerGroup(mixerGroupArray[0]);
							}
							pAudio.Play ();
							if(objectPool.eventMode > 0)
								PoolMasterEvents.EventPlayAudio(audioNames[i], obj);
						}
					}
				}
		}

		public void StopAllAudio(){
			objectPool.Despawn("ADVANCED AUDIO");
		}

		public void StopAudio(string clipName){
			if(audioNames.Contains(clipName)){
				foreach(GameObject g in objectPool.GetPool("ADVANCED AUDIO").pool){
					PooledAudio pAudio = g.GetComponent<PooledAudio>();
					if(pAudio !=null){
						if(pAudio.ClipIndex == audioNames.IndexOf(clipName)){
							g.GetComponent<AudioSource>().Stop();
							objectPool.Despawn(g);
						}
					}
				}
			}
		}
	}
}
