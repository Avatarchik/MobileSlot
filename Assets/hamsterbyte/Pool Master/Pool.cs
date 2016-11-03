/*

**************************************
************ POOL MASTER *************
**************************************
______________________________________

VERSION: 3.0
FILE:    POOL.CS
AUTHOR:  CODY JOHNSON
COMPANY: HAMSTERBYTE, LLC
EMAIL:   HAMSTERBYTELLC@GMAIL.COM
WEBSITE: WWW.HAMSTERBYTE.COM
SUPPORT: WWW.HAMSTERBYTE.COM/POOL-MASTER

COPYRIGHT © 2014-2015 HAMSTERBYTE, LLC
ALL RIGHTS RESERVED

*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace hamsterbyte.PoolMaster {
	/// <summary>
	///The Pool class refers to the pools that are controlled by PoolMaster
	///Pools can be created with the constructors below using several different paramaters 
	/// </summary>
	[System.Serializable]
	public class Pool {

		public string name;
		public List<GameObject> uniquePool;
		public int maxBuffer;
		public List<int> bufferAmount;
		public List<GameObject> pool;
		public List<bool> alwaysMax;
		public List<int> activeCount = new List<int>();
		public int totalActive;
		public bool preload;
		public bool persistent;
		public bool hasLoaded;
		public bool hide;


		public Pool () {

			//Initialize a new instance of the pool class with the name "Default"
			//This is useful if you plan to use a single pool of objects. This is not reccomended if you are pooling a lot of different objects
			//If you plan to use more than one pool of objects, you must use one of the other constructors as they allow you to name the pool

			name = "New Pool";
			pool = new List<GameObject> ();
			uniquePool = new List<GameObject> ();
			bufferAmount = new List<int> ();
			alwaysMax = new List<bool> ();
			maxBuffer = 25;
			preload = false;
			persistent = false;
			hasLoaded = false;
			hide = false;

		}
        
		public Pool (string poolName) {

			//Initializes a new instance of the Pool class with a specified name.
			//Specifying the name allows for grouping of similar GameObjects; i.e enemies, particles, sounds, etc

			name = poolName;
			pool = new List<GameObject> ();
			uniquePool = new List<GameObject> ();
			bufferAmount = new List<int> ();
			alwaysMax = new List<bool> ();
			maxBuffer = 25;
			preload = false;
			persistent = false;
			hasLoaded = false;
			hide = false;
		}
        
		public Pool (string poolName, int buffer, bool p, bool pr, bool h) {	
			name = poolName;
			pool = new List<GameObject> ();
			uniquePool = new List<GameObject> ();
			bufferAmount = new List<int> ();
			bufferAmount.Add (buffer);
			alwaysMax = new List<bool> ();
			maxBuffer = buffer;
			preload = p;
			persistent = pr;
			hasLoaded = false;
			hide = h;
		}
	}
}