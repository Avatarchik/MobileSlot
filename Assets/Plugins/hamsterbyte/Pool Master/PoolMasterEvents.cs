/*

**************************************
************ POOL MASTER *************
**************************************
______________________________________

VERSION: 3.0
FILE:    POOLMASTEREVENTS.CS
AUTHOR:  CODY JOHNSON
COMPANY: HAMSTERBYTE, LLC
EMAIL:   HAMSTERBYTELLC@GMAIL.COM
WEBSITE: WWW.HAMSTERBYTE.COM
SUPPORT: WWW.HAMSTERBYTE.COM/POOL-MASTER

COPYRIGHT © 2014 HAMSTERBYTE, LLC
ALL RIGHTS RESERVED

*/
using UnityEngine;
using System.Collections;

public static class PoolMasterEvents {

	#region DELEGATES
	public delegate void OnSpawn (GameObject g);
	public delegate void OnDespawnObject (GameObject g);
	public delegate void OnDespawnPool (string poolName);
	public delegate void OnDestroyPool (string poolName);
	public delegate void OnDestroyObject (GameObject g);
	public delegate void OnPreloadPool (string poolName);
	public delegate void OnPlayAudio (string clipName, GameObject sourceObject);
	#endregion
	
	#region EVENTS
	public static event OnSpawn onSpawn;
	public static event OnDespawnObject onDespawnObject;
	public static event OnDespawnPool onDespawnPool;
	public static event OnDestroyPool onDestroyPool;
	public static event OnDestroyObject onDestroyObject;
	public static event OnPreloadPool onPreloadPool;
	public static event OnPlayAudio onPlayAudio;
	#endregion
	
	#region EVENT TRIGGERS
	public static void EventSpawn (GameObject g) {
		if (onSpawn != null)
			onSpawn (g);
	}
	
	public static void EventDespawnObject (GameObject g) {
		if (onDespawnObject != null)
			onDespawnObject (g);
	}
	
	public static void EventDespawnPool (string poolName) {
		if (onDespawnPool != null)
			onDespawnPool (poolName);
	}
	
	public static void EventDestroyPool (string poolName) {
		if (onDestroyPool != null)
			onDestroyPool (poolName);
	}
	
	public static void EventDestroyObject (GameObject g) {
		if (onDestroyObject != null)
			onDestroyObject (g);
	}
	
	public static void EventPreloadPool (string poolName) {
		if (onPreloadPool != null)
			onPreloadPool (poolName);
	}

	public static void EventPlayAudio(string clipName, GameObject sourceObject){
		if(onPlayAudio != null)
			onPlayAudio(clipName, sourceObject);
	}
	#endregion
	
}
