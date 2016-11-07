/*

**************************************
************ POOL MASTER *************
**************************************
______________________________________

VERSION: 3.0
FILE:    POOLEDAUDIO.CS
AUTHOR:  CODY JOHNSON
COMPANY: HAMSTERBYTE, LLC
EMAIL:   HAMSTERBYTELLC@GMAIL.COM
WEBSITE: WWW.HAMSTERBYTE.COM
SUPPORT: WWW.HAMSTERBYTE.COM/POOL-MASTER

COPYRIGHT © 2014 HAMSTERBYTE, LLC
ALL RIGHTS RESERVED

Fix OnLevelWasLoaded deprecated waring by lpesign
*/
using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using UnityEngine.SceneManagement;

namespace hamsterbyte.PoolMaster {
	[RequireComponent (typeof(AudioSource))]
	public class PooledAudio : MonoBehaviour {
		public bool playOnSpawn = true;
		private bool _loaded;
		public PoolMasterAudio audioMaster;
		private int _clipIndex;
		public int ClipIndex {get {return _clipIndex;} set {_clipIndex = value;} }
		private bool _persistent;
		void Awake () {
			//Has the sound been loaded into memory yet? If not, we make sure it is disabled to refrain from playing
			//the sound until it has been loaded and called upon.
			if (!_loaded) {
				if (this.gameObject.activeSelf)
					this.gameObject.SetActive (false);
				_loaded = true;
			}
		}
        
		void OnEnable () {
			if (playOnSpawn) {
				Play ();
			}

			SceneManager.sceneLoaded += OnLevelFinishedLoading;
		}

		void OnDisable(){
			if(audioMaster != null && _loaded){
				audioMaster.activeCount[_clipIndex]--;
				audioMaster.activeClips--;
				if(audioMaster.activeClips < 0)
					audioMaster.activeClips = 0;
				if(audioMaster.activeCount[_clipIndex] < 0)
					audioMaster.activeCount[_clipIndex] = 0;
			}

			SceneManager.sceneLoaded -= OnLevelFinishedLoading;
		}

		void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
		{
			if(!_persistent){
				this.gameObject.SetActive(false);
			}
		}

		/*
		void OnLevelWasLoaded(int index){
			if(!_persistent){
				this.gameObject.SetActive(false);
			}
		}
		*/
        
		public void Define (AudioClip clip, float volume, float pan, float pitch, int priority, bool loop, bool persistence) {

			if(audioMaster != null){
				_clipIndex = audioMaster.audioClips.IndexOf(clip);
			}

			AudioSource source = this.GetComponent<AudioSource> ();
			if (source != null) {
				source.clip = clip;
				source.volume = volume;
				source.panStereo = pan;
				source.priority = priority;
				source.loop = loop;
				source.pitch = pitch;
				_persistent = persistence;
			}
		}

		public void SetMixerGroup(AudioMixerGroup mixerGroup) {
			if(audioMaster != null){
				if(audioMaster.audioMixer != null){
					AudioSource source = this.GetComponent<AudioSource> ();
					if(source != null){
						source.outputAudioMixerGroup = mixerGroup;
					}
				}
			}
		}
        
		public void Play () {
			if(this.gameObject.activeSelf){
				StartCoroutine ("PlayAudio");
			} else {
				gameObject.SetActive(true);
				StartCoroutine ("PlayAudio");
			}
		}
        
		public IEnumerator PlayAudio () {
			//Find the audio source attached to this object and play it.
			AudioSource a = this.GetComponent<AudioSource> ();
			a.Play ();
			if(audioMaster != null){
				audioMaster.activeCount[_clipIndex]++;
				audioMaster.activeClips++;
			}

			//If the audio is called to loop we skip this, if not the object is automatically disabled after the audio has finished playing
			if (!a.loop) {
				yield return new WaitForSeconds (a.clip.length);
				this.gameObject.SetActive (false);
			}

			StopAllCoroutines ();
		}
	}
}
