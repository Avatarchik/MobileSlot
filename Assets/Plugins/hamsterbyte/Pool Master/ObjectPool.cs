/*

**************************************
************ POOL MASTER *************
**************************************
______________________________________

VERSION: 3.0
FILE:    OBJECTPOOL.CS
AUTHOR:  CODY JOHNSON
COMPANY: HAMSTERBYTE, LLC
EMAIL:   HAMSTERBYTELLC@GMAIL.COM
WEBSITE: WWW.HAMSTERBYTE.COM
SUPPORT: WWW.HAMSTERBYTE.COM/POOL-MASTER

COPYRIGHT © 2014-2015 HAMSTERBYTE, LLC
ALL RIGHTS RESERVED


Fix OnLevelWasLoaded deprecated waring by lpesign
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace hamsterbyte.PoolMaster
{
    [System.Serializable]
    public class ObjectPool : MonoBehaviour
    {
        #region VARIABLES
        public List<Pool> pools;
        public string[] PreloadModes = new string[] { "None", "Selective", "All" };
        public int preloadMode = 2;
        public string[] PersistenceModes = new string[] { "Off", "Selective", "All" };
        public int persistenceMode;
        public string[] SmartBufferModes = new string[] { "Off", "On" };
        public int smartBufferMode;
        public int smartBufferMax = 255;
        private int smartBufferCount;
        public string[] DebugModes = new string[] { "Off", "Normal", "Complete" };
        public int debugMode;
        public string[] HideModes = new string[] { "Off", "Selective", "All" };
        public int hideMode;
        public string[] EventModes = new string[] { "Off", "On" };
        public int eventMode;
        public int totalActive;

        public PoolMasterAudio audioPool
        {
            get
            {
                return this.GetComponent<PoolMasterAudio>();
            }
        }

        public PoolMasterSpawnPoints spawnPoints
        {
            get
            {
                return this.GetComponent<PoolMasterSpawnPoints>();
            }
        }

        public static ObjectPool Instance { get; private set; }
        #endregion

        #region CONSTRUCTOR
        public ObjectPool()
        {
            pools = new List<Pool>();
        }
        #endregion

        #region MONO METHODS
        void Awake()
        {
            CheckPrecedent();
            if (audioPool != null)
                PreloadAudio();
            if (preloadMode == 2)
                PreloadGlobal();
            if (preloadMode == 1)
                PreloadLocal();
        }


        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinishedLoading;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        }

        void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            if (persistenceMode == 1)
            {
                foreach (Pool p in pools)
                {
                    if (!p.persistent)
                        this.Destroy(p.name);
                }
            }
        }

        /*
		void OnLevelWasLoaded (int level) {
			if (persistenceMode == 1) {
				foreach (Pool p in pools) {
					if(!p.persistent)
						this.Destroy (p.name);
				}
			}
		}
		*/
        #endregion

        #region CHECK PRECEDENT
        private void CheckPrecedent()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            Instance = this;
            if (persistenceMode > 0)
                DontDestroyOnLoad(gameObject);
        }
        #endregion

        #region PRELOAD GLOBAL
        public void PreloadGlobal()
        {
            foreach (Pool p in pools)
                Preload(p.name);
        }
        #endregion

        #region PRELOAD LOCAL
        public void PreloadLocal()
        {
            foreach (Pool p in pools)
            {
                if (p.preload)
                    Preload(p.name);
            }
        }
        #endregion

        #region PRELOAD
        /// <summary>
        /// Preload the specified poolName.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        public void Preload(string poolName)
        {
            Pool p = GetPool(poolName);
            if (p != null && !p.hasLoaded)
            {
                if (eventMode > 0)
                    PoolMasterEvents.EventPreloadPool(p.name);
                GameObject parent = new GameObject(p.name);
                parent.transform.parent = this.transform;
                if (hideMode == 2)
                    parent.hideFlags = HideFlags.HideInHierarchy;
                if (hideMode == 1)
                {
                    if (p.hide)
                        parent.hideFlags = HideFlags.HideInHierarchy;
                }

                p.hasLoaded = true;
                if (debugMode > 1 && Application.isEditor)
                    Debug.Log("Preloading Pool: " + p.name);

                if (p.uniquePool.Count > 0)
                {
                    for (int i = 0; i < p.uniquePool.Count; i++)
                    {
                        if (p.activeCount != null)
                        {
                            p.activeCount.Add(0);
                        }
                        else
                        {
                            p.activeCount = new List<int>();
                            p.activeCount.Add(0);
                        }
                        for (int j = 0; j < p.bufferAmount[i]; j++)
                        {
                            if (p.uniquePool[i] != null)
                            {

                                //This instantiates the prefabs in the object pool
                                //Then gives them a unique name; the unique name is for reference only and it is not used to actually call the object.
                                //To call the object you use the original name of the prefab
                                //Lastly, it sets the parent of the new object to the parent object for the respective pool
                                GameObject g = (GameObject)Instantiate(p.uniquePool[i]);
                                PooledObject pObject = g.AddComponent<PooledObject>();
                                pObject.Define(this, parent.transform, pools.IndexOf(p), i);
                                g.name = g.name.Replace(' ', '_');
                                g.name = g.name.Split('(')[0] + " - " + j.ToString();
                                g.transform.parent = parent.transform;

                                //If the prefab contains an AudioSource, check to see that the PooledAudio script is attached to it.
                                //If it does not already contain a PooledAudio script we will add a new one.
                                //This script is neccessary to control the behaviour of the audio source with Pool Master
                                if (g.GetComponent<AudioSource>() != null)
                                {
                                    if (g.GetComponent<PooledAudio>() == null)
                                    {
                                        g.AddComponent<PooledAudio>();
                                    }
                                }

                                //If the prefab contains a ParticleSystem, check to see that the PooledParticlescript is attached to it.
                                //If it does not already contain a PooledParticle script we will add a new one.
                                //This script is neccessary to control the behaviour of the particle system with Pool Master
                                if (g.GetComponent<ParticleSystem>() != null)
                                {
                                    if (g.GetComponent<PooledParticle>() == null)
                                    {
                                        g.AddComponent<PooledParticle>();
                                    }
                                }

                                //Despawn the object for use later. Objects in the pool are Despawnd by default to certify performance. Too many active objects will cause performace issues in your game.
                                //The performance issues are directly related to how intensive the scripts attached to the prefab are. More intensive scripts create a heavier load on the processor.
                                Despawn(g, true);

                                //This is the section that actually checks if there is another pool with the same name already present in the scene.
                                Pool pCheck = GetPool(p.name);
                                pCheck.pool.Add(g);
                                pObject.hasLoaded = true;

                            }
                        }

                    }
                }
            }
            else
            {
                if (p == null && debugMode > 0 && Application.isEditor)
                    Debug.LogWarning("Preload: Cannot find pool named '" + poolName + "'. Please verify spelling.");
            }
        }
        #endregion

        #region PRELOAD AUDIO
        private void PreloadAudio()
        {
            if (audioPool != null)
            {
                pools.Add(new Pool("ADVANCED AUDIO", audioPool.audioBuffer, true, true, false));
                GameObject tObj = new GameObject("ADVANCED AUDIO");
                tObj.AddComponent<PooledAudio>();
                PooledAudio tPooledAudio = tObj.GetComponent<PooledAudio>();
                tPooledAudio.playOnSpawn = false;
                tPooledAudio.audioMaster = audioPool;
                tObj.hideFlags = HideFlags.HideInHierarchy;
                tObj.transform.parent = Instance.transform;
                tObj.GetComponent<AudioSource>().playOnAwake = false;
                GetPool("ADVANCED AUDIO").uniquePool.Add(tObj);
                Preload("ADVANCED AUDIO");
                this.transform.FindChild("ADVANCED AUDIO").gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }
        #endregion

        #region STOP AUDIO
        public void StopAudio(string clipName)
        {
            if (audioPool != null)
                audioPool.StopAudio(clipName);
        }

        public void StopAllAudio()
        {
            if (audioPool != null)
                audioPool.StopAllAudio();
        }
        #endregion

        #region PLAY AUDIO
        public void PlayAudio(string clipName)
        {
            if (audioPool != null)
            {
                if (audioPool.audioNames.Contains(clipName))
                {
                    audioPool.PlayAudio(SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO"), clipName);
                }
                else
                {
                    if (debugMode > 0)
                        Debug.LogWarning("PlayAudio: Clip '" + clipName + "' does not exist!");
                }
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
        }

        public void PlayAudio(string clipName, Vector3 position)
        {
            if (audioPool != null)
            {
                if (audioPool.audioNames.Contains(clipName))
                {
                    audioPool.PlayAudio(SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO", position), clipName);
                }
                else
                {
                    if (debugMode > 0)
                        Debug.LogWarning("PlayAudio: Clip '" + clipName + "' does not exist!");
                }
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
        }

        public void PlayAudio(string clipName, bool loop)
        {
            if (audioPool != null)
            {
                if (audioPool.audioNames.Contains(clipName))
                {
                    audioPool.PlayAudio(SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO"), clipName, loop);
                }
                else
                {
                    if (debugMode > 0)
                        Debug.LogWarning("PlayAudio: Clip '" + clipName + "' does not exist!");
                }
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
        }

        public void PlayAudio(string clipName, Vector3 position, bool loop)
        {
            if (audioPool != null)
            {
                if (audioPool.audioNames.Contains(clipName))
                {
                    audioPool.PlayAudio(SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO", position), clipName, loop);
                }
                else
                {
                    if (debugMode > 0)
                        Debug.LogWarning("PlayAudio: Clip '" + clipName + "' does not exist!");
                }
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
        }

        public void PlayAudio(int index)
        {
            if (audioPool != null)
            {
                audioPool.PlayAudio(SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO"), index);
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
        }

        public void PlayAudio(int index, Vector3 position)
        {
            if (audioPool != null)
            {
                audioPool.PlayAudio(SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO", position), index);
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
        }

        public void PlayAudio(int index, bool loop)
        {
            if (audioPool != null)
            {
                audioPool.PlayAudio(SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO"), index, loop);
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
        }

        public void PlayAudio(int index, Vector3 position, bool loop)
        {
            if (audioPool != null)
            {
                audioPool.PlayAudio(SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO", position), index, loop);
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
        }

        #endregion

        #region PLAY AUDIO REFERENCE
        public GameObject PlayAudioReference(string clipName)
        {
            GameObject r = SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO");
            if (audioPool != null)
            {
                if (audioPool.audioNames.Contains(clipName))
                {
                    audioPool.PlayAudio(r, clipName);
                }
                else
                {
                    if (debugMode > 0)
                        Debug.LogWarning("PlayAudio: Clip '" + clipName + "' does not exist!");
                }
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
            return r;
        }

        public GameObject PlayAudioReference(string clipName, Vector3 position)
        {
            GameObject r = SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO", position);
            if (audioPool != null)
            {
                if (audioPool.audioNames.Contains(clipName))
                {
                    audioPool.PlayAudio(r, clipName);
                }
                else
                {
                    if (debugMode > 0)
                        Debug.LogWarning("PlayAudio: Clip '" + clipName + "' does not exist!");
                }
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
            return r;
        }

        public GameObject PlayAudioReference(string clipName, bool loop)
        {
            GameObject r = SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO");
            if (audioPool != null)
            {
                if (audioPool.audioNames.Contains(clipName))
                {
                    audioPool.PlayAudio(r, clipName, loop);
                }
                else
                {
                    if (debugMode > 0)
                        Debug.LogWarning("PlayAudioLoop: Clip '" + clipName + "' does not exist!");
                }
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudioLoop: Audio pool does not exist!");
            }
            return r;
        }

        public GameObject PlayAudioReference(string clipName, Vector3 position, bool loop)
        {
            GameObject r = SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO", position);
            if (audioPool != null)
            {
                if (audioPool.audioNames.Contains(clipName))
                {
                    audioPool.PlayAudio(r, clipName, loop);
                }
                else
                {
                    if (debugMode > 0)
                        Debug.LogWarning("PlayAudioLoop: Clip '" + clipName + "' does not exist!");
                }
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudioLoop: Audio pool does not exist!");
            }
            return r;
        }

        public GameObject PlayAudioReference(int index)
        {
            GameObject r = SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO");
            if (audioPool != null)
            {
                audioPool.PlayAudio(r, index);
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
            return r;
        }

        public GameObject PlayAudioReference(int index, Vector3 position)
        {
            GameObject r = SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO", position);
            if (audioPool != null)
            {
                audioPool.PlayAudio(r, index);
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
            return r;
        }

        public GameObject PlayAudioReference(int index, bool loop)
        {
            GameObject r = SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO");
            if (audioPool != null)
            {
                audioPool.PlayAudio(r, index, loop);
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
            return r;
        }

        public GameObject PlayAudioReference(int index, Vector3 position, bool loop)
        {
            GameObject r = SpawnReference("ADVANCED AUDIO", "ADVANCED AUDIO", position);
            if (audioPool != null)
            {
                audioPool.PlayAudio(r, index, loop);
            }
            else
            {
                if (debugMode > 0)
                    Debug.LogWarning("PlayAudio: Audio pool does not exist!");
            }
            return r;
        }
        #endregion

        #region GET SPAWN POINT
        public Vector3 GetSpawnPoint(string groupName, int index)
        {
            Vector3 point = Vector3.zero;
            if (spawnPoints != null)
            {
                try
                {
                    for (int i = 0; i < spawnPoints.spawnGroups.Count; i++)
                    {
                        if (spawnPoints.spawnGroups[i].name == groupName)
                        {
                            try
                            {
                                point = spawnPoints.spawnGroups[i].spawnPoints[index];
                            }
                            catch (System.Exception e)
                            {
                                Debug.Log("GetSpawnPoint: " + e.Message);
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (debugMode != 0)
                        Debug.Log("GetSpawnPoint: " + e.Message);
                }
            }
            else
            {
                Debug.Log("No spawn points associated with Pool Master object. Please add the Pool Master Spawn Points component via the inspector.");
                return Vector3.zero;
            }
            return point;

        }
        #endregion

        #region GET SPAWN POINTS
        public List<Vector3> GetSpawnPoints(string groupName)
        {
            List<Vector3> points = new List<Vector3>();
            if (spawnPoints != null)
            {
                try
                {
                    for (int i = 0; i < spawnPoints.spawnGroups.Count; i++)
                    {
                        if (spawnPoints.spawnGroups[i].name == groupName)
                        {
                            try
                            {
                                points.AddRange(spawnPoints.spawnGroups[i].spawnPoints);
                            }
                            catch (System.Exception e)
                            {
                                Debug.Log("GetSpawnPoints: " + e.Message);
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (debugMode != 0)
                        Debug.Log("GetSpawnPoints: " + e.Message);
                }
            }
            else
            {
                Debug.Log("No spawn points associated with Pool Master object. Please add the Pool Master Spawn Points component via the inspector.");
            }
            return points;

        }

        public List<Vector3> GetSpawnPoints(List<string> groupNames)
        {
            List<Vector3> points = new List<Vector3>();
            if (spawnPoints != null)
            {
                try
                {
                    foreach (string s in groupNames)
                    {
                        for (int i = 0; i < spawnPoints.spawnGroups.Count; i++)
                        {
                            if (spawnPoints.spawnGroups[i].name == s)
                            {
                                try
                                {
                                    points.AddRange(spawnPoints.spawnGroups[i].spawnPoints);
                                }
                                catch (System.Exception e)
                                {
                                    Debug.Log("GetSpawnPoints: " + e.Message);
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (debugMode != 0)
                        Debug.Log("GetSpawnPoints: " + e.Message);
                }
            }
            else
            {
                Debug.Log("No spawn points associated with Pool Master object. Please add the Pool Master Spawn Points component via the inspector.");
            }
            return points;

        }

        public List<Vector3> GetSpawnPoints(string[] groupNames)
        {
            List<Vector3> points = new List<Vector3>();
            if (spawnPoints != null)
            {
                try
                {
                    foreach (string s in groupNames)
                    {
                        for (int i = 0; i < spawnPoints.spawnGroups.Count; i++)
                        {
                            if (spawnPoints.spawnGroups[i].name == s)
                            {
                                try
                                {
                                    points.AddRange(spawnPoints.spawnGroups[i].spawnPoints);
                                }
                                catch (System.Exception e)
                                {
                                    Debug.Log("GetSpawnPoints: " + e.Message);
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (debugMode != 0)
                        Debug.Log("GetSpawnPoints: " + e.Message);
                }
            }
            else
            {
                Debug.Log("No spawn points associated with Pool Master object. Please add the Pool Master Spawn Points component via the inspector.");
            }
            return points;

        }
        #endregion

        #region GET RANDOM SPAWN POINT
        public Vector3 GetRandomSpawnPoint(string groupName)
        {
            List<Vector3> points = new List<Vector3>();
            if (spawnPoints != null)
            {
                try
                {
                    for (int i = 0; i < spawnPoints.spawnGroups.Count; i++)
                    {
                        if (spawnPoints.spawnGroups[i].name == groupName)
                        {
                            try
                            {
                                points.AddRange(spawnPoints.spawnGroups[i].spawnPoints);
                            }
                            catch (System.Exception e)
                            {
                                Debug.Log("GetSpawnPoints: " + e.Message);
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (debugMode != 0)
                        Debug.Log("GetSpawnPoints: " + e.Message);
                }
            }
            else
            {
                Debug.Log("No spawn points associated with Pool Master object. Please add the Pool Master Spawn Points component via the inspector.");
            }
            if (points.Count > 0)
            {
                return points[Random.Range(0, points.Count)];
            }
            else
            {
                Debug.Log("GetRandomSpawnPoint: No spawn points found");
                return Vector3.zero;
            }

        }

        public Vector3 GetRandomSpawnPoint(List<string> groupNames)
        {
            List<Vector3> points = new List<Vector3>();
            if (spawnPoints != null)
            {
                try
                {
                    foreach (string s in groupNames)
                    {
                        for (int i = 0; i < spawnPoints.spawnGroups.Count; i++)
                        {
                            if (spawnPoints.spawnGroups[i].name == s)
                            {
                                try
                                {
                                    points.AddRange(spawnPoints.spawnGroups[i].spawnPoints);
                                }
                                catch (System.Exception e)
                                {
                                    Debug.Log("GetSpawnPoints: " + e.Message);
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (debugMode != 0)
                        Debug.Log("GetSpawnPoints: " + e.Message);
                }
            }
            else
            {
                Debug.Log("No spawn points associated with Pool Master object. Please add the Pool Master Spawn Points component via the inspector.");
            }
            if (points.Count > 0)
            {
                return points[Random.Range(0, points.Count)];
            }
            else
            {
                Debug.Log("GetRandomSpawnPoint: No spawn points found");
                return Vector3.zero;
            }

        }

        public Vector3 GetRandomSpawnPoint(string[] groupNames)
        {
            List<Vector3> points = new List<Vector3>();
            if (spawnPoints != null)
            {
                try
                {
                    foreach (string s in groupNames)
                    {
                        for (int i = 0; i < spawnPoints.spawnGroups.Count; i++)
                        {
                            if (spawnPoints.spawnGroups[i].name == s)
                            {
                                try
                                {
                                    points.AddRange(spawnPoints.spawnGroups[i].spawnPoints);
                                }
                                catch (System.Exception e)
                                {
                                    Debug.Log("GetSpawnPoints: " + e.Message);
                                }
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (debugMode != 0)
                        Debug.Log("GetSpawnPoints: " + e.Message);
                }
            }
            else
            {
                Debug.Log("No spawn points associated with Pool Master object. Please add the Pool Master Spawn Points component via the inspector.");
            }
            if (points.Count > 0)
            {
                return points[Random.Range(0, points.Count)];
            }
            else
            {
                Debug.Log("GetRandomSpawnPoint: No spawn points found");
                return Vector3.zero;
            }

        }
        #endregion

        #region ADD SPAWN POINT
        public void AddSpawnPoint(string groupName, Vector3 point)
        {
            if (spawnPoints != null)
            {
                SpawnGroup g = null;
                for (int i = 0; i < spawnPoints.spawnGroups.Count; i++)
                {
                    if (spawnPoints.spawnGroups[i].name == groupName)
                    {
                        g = spawnPoints.spawnGroups[i];
                    }
                }
                if (g != null)
                {
                    g.AddPoint(point);
                }
                else
                {
                    spawnPoints.AddGroup(groupName);
                    spawnPoints.spawnGroups[spawnPoints.spawnGroups.Count - 1].AddPoint(point);
                }
            }
        }
        #endregion

        #region SPAWN
        /// <summary>
        /// Spawn the specified object from a specified pool
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="objName">Object name.</param>
        public void Spawn(string poolName, string objName)
        {
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                GameObject tObj = GetUniqueObject(pCheck, objName);
                if (tObj != null)
                {
                    GameObject tPoolObj = GetInactiveObject(pCheck, tObj);
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        if (tPoolObj.GetComponent<AudioSource>() != null)
                            tPoolObj.transform.position = Camera.main.transform.position;

                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("Spawn: '" + tObj.name + "' spawned successfully.");
                    }
                    else
                    {
                        if (smartBufferMode > 0)
                        {
                            SmartBuffer(poolName, tObj);
                        }
                        else
                        {
                            if (Application.isEditor && debugMode > 0)
                            {
                                Debug.Log("Spawn: '" + objName + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                            }
                        }
                    }
                }
                else
                {
                    if (Application.isEditor && debugMode > 0)
                        Debug.LogWarning("Spawn: '" + objName + "' not found! Please verify spelling.");
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("Spawn: Pool -> '" + poolName + "' not found! Please verify spelling.");
            }
        }

        /// <summary>
        /// Spawn the specified poolName, objName and position.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="objName">Object name.</param>
        /// <param name="position">Position.</param>
        public void Spawn(string poolName, string objName, Vector3 position)
        {
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                GameObject tObj = GetUniqueObject(pCheck, objName);
                if (tObj != null)
                {
                    GameObject tPoolObj = GetInactiveObject(pCheck, tObj);
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("Spawn: '" + tObj.name + "' spawned successfully.");
                    }
                    else
                    {
                        if (smartBufferMode > 0)
                        {
                            SmartBuffer(poolName, tObj, position);
                        }
                        else
                        {
                            if (Application.isEditor && debugMode > 0)
                            {
                                Debug.Log("Spawn: '" + objName + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                            }
                        }
                    }
                }
                else
                {
                    if (Application.isEditor && debugMode > 0)
                        Debug.LogWarning("Spawn: '" + objName + "' not found! Please verify name.");
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("Spawn: Pool -> '" + poolName + "' not found! Please verify spelling.");
            }
        }

        /// <summary>
        /// Spawn the specified poolName, objName, position and rotation.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="objName">Object name.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        public void Spawn(string poolName, string objName, Vector3 position, Quaternion rotation)
        {
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                GameObject tObj = GetUniqueObject(pCheck, objName);
                if (tObj != null)
                {
                    GameObject tPoolObj = GetInactiveObject(pCheck, tObj);
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.transform.rotation = rotation;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("Spawn: '" + tObj.name + "' spawned successfully.");
                    }
                    else
                    {
                        if (smartBufferMode > 0)
                        {
                            SmartBuffer(poolName, tObj, position, rotation);
                        }
                        else
                        {
                            if (Application.isEditor && debugMode > 0)
                            {
                                Debug.Log("Spawn: '" + objName + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                            }
                        }
                    }

                }
                else
                {
                    if (Application.isEditor && debugMode > 0)
                        Debug.LogWarning("Spawn: '" + objName + "' not found! Please verify name.");
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("Spawn: Pool -> '" + poolName + "' not found! Please verify spelling.");
            }
        }
        #endregion

        #region SPAWN REFERENCE
        /// <summary>
        /// Spawns object with given pool name and object name; returns a reference.
        /// </summary>
        /// <returns>The reference.</returns>
        /// <param name="poolName">Pool name.</param>
        /// <param name="objName">Object name.</param>
        public GameObject SpawnReference(string poolName, string objName)
        {
            GameObject tPoolObj = null;
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                GameObject tObj = GetUniqueObject(pCheck, objName);
                if (tObj != null)
                {
                    tPoolObj = GetInactiveObject(pCheck, tObj);
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        if (tPoolObj.GetComponent<AudioSource>() != null)
                            tPoolObj.transform.position = Camera.main.transform.position;

                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnReference: Object -> '" + tObj.name + "' spawned successfully.");
                    }
                    else
                    {
                        if (smartBufferMode > 0)
                        {
                            tPoolObj = SmartBufferReference(poolName, tObj);
                        }
                        else
                        {
                            if (Application.isEditor && debugMode > 0)
                            {
                                Debug.Log("SpawnReference: '" + objName + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                            }
                        }
                    }
                }
                else
                {
                    if (Application.isEditor && debugMode > 0)
                        Debug.LogWarning("Spawn: '" + objName + "' not found! Please verify spelling.");
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("SpawnReference: Pool -> '" + poolName + "' not found! Please verify spelling.");
            }
            return tPoolObj;
        }

        /// <summary>
        /// Spawns object with given pool name, object name, and position; returns a reference.
        /// </summary>
        /// <returns>The reference.</returns>
        /// <param name="poolName">Pool name.</param>
        /// <param name="objName">Object name.</param>
        /// <param name="position">Position.</param>
        public GameObject SpawnReference(string poolName, string objName, Vector3 position)
        {
            GameObject tPoolObj = null;
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                GameObject tObj = GetUniqueObject(pCheck, objName);
                if (tObj != null)
                {
                    tPoolObj = GetInactiveObject(pCheck, tObj);
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnReference: Object -> '" + tObj.name + "' spawned successfully.");
                    }
                    else
                    {
                        if (smartBufferMode > 0)
                        {
                            tPoolObj = SmartBufferReference(poolName, tObj, position);
                        }
                        else
                        {
                            if (Application.isEditor && debugMode > 0)
                            {
                                Debug.Log("SpawnReference: '" + objName + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                            }
                        }
                    }
                }
                else
                {
                    if (Application.isEditor && debugMode > 0)
                        Debug.LogWarning("Spawn: '" + objName + "' not found! Verify name or increase object buffer.");
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("SpawnReference: Pool -> '" + poolName + "' not found! Please verify spelling.");
            }
            return tPoolObj;
        }

        /// <summary>
        /// Spawns object with given pool name, object name, position, and rotation; returns a reference.
        /// </summary>
        /// <returns>The reference.</returns>
        /// <param name="poolName">Pool name.</param>
        /// <param name="objName">Object name.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        public GameObject SpawnReference(string poolName, string objName, Vector3 position, Quaternion rotation)
        {
            GameObject tPoolObj = null;
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                GameObject tObj = GetUniqueObject(pCheck, objName);
                if (tObj != null)
                {
                    tPoolObj = GetInactiveObject(pCheck, tObj);
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.transform.rotation = rotation;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnReference: Object -> '" + tObj.name + "' spawned successfully.");
                    }
                    else
                    {
                        if (smartBufferMode > 0)
                        {
                            tPoolObj = SmartBufferReference(poolName, tObj, position, rotation);
                        }
                        else
                        {
                            if (Application.isEditor && debugMode > 0)
                            {
                                Debug.Log("SpawnReference: '" + objName + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                            }
                        }
                    }
                }
                else
                {
                    if (Application.isEditor && debugMode > 0)
                        Debug.LogWarning("Spawn: '" + objName + "' not found! Verify name or increase object buffer.");
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("SpawnReference: Pool -> '" + poolName + "' not found! Please verify spelling.");
            }
            return tPoolObj;
        }
        #endregion

        #region SPAWN RANDOM
        /// <summary>
        /// Spawns random object with given pool name and position.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="position">Position.</param>
        public void SpawnRandom(string poolName, Vector3 position)
        {
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                List<GameObject> inactive = FindInactive(pCheck.pool);
                if (inactive.Count > 0)
                {
                    GameObject tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        SmartBuffer(poolName, tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandom: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("SpawnRandom: Pool -> '" + poolName + "' not found. Please verify spelling");
            }
        }
        /// <summary>
        /// Spawns random object from given list of pool names and position.
        /// </summary>
        /// <param name="poolNames">Pool names.</param>
        /// <param name="position">Position.</param>
        public void SpawnRandom(List<string> poolNames, Vector3 position)
        {

            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> inactive = FindInactive(MergedPool(poolNames));
                if (inactive.Count > 0)
                {
                    GameObject tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    int i = Random.Range(0, poolNames.Count);
                    Pool pCheck = GetPool(poolNames[i]);
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        SmartBuffer(poolNames[i], tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandom: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Spawns random object from given array of pool names and position.
        /// </summary>
        /// <param name="poolNames">Pool names.</param>
        /// <param name="position">Position.</param>
        public void SpawnRandom(string[] poolNames, Vector3 position)
        {

            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> inactive = FindInactive(MergedPool(poolNames));
                if (inactive.Count > 0)
                {
                    GameObject tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    int i = Random.Range(0, poolNames.Length);
                    Pool pCheck = GetPool(poolNames[i]);
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        SmartBuffer(poolNames[i], tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandom: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Spawns random object with given pool name, position, and rotation.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        public void SpawnRandom(string poolName, Vector3 position, Quaternion rotation)
        {
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                List<GameObject> inactive = FindInactive(pCheck.pool);
                if (inactive.Count > 0)
                {
                    GameObject tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.transform.rotation = rotation;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        SmartBuffer(poolName, tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandom: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("SpawnRandom: Pool -> '" + poolName + "' not found. Please verify spelling");
            }
        }
        /// <summary>
        /// Spawns random object from given list of pool names, position, and rotation.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        public void SpawnRandom(List<string> poolNames, Vector3 position, Quaternion rotation)
        {
            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> inactive = FindInactive(MergedPool(poolNames));
                if (inactive.Count > 0)
                {
                    GameObject tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.transform.rotation = rotation;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    int i = Random.Range(0, poolNames.Count);
                    Pool pCheck = GetPool(poolNames[i]);
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        SmartBuffer(poolNames[i], tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandom: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Spawns random object from given array of pool names, position, and rotation.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        public void SpawnRandom(string[] poolNames, Vector3 position, Quaternion rotation)
        {
            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> inactive = FindInactive(MergedPool(poolNames));
                if (inactive.Count > 0)
                {

                    GameObject tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.transform.rotation = rotation;
                        tPoolObj.SetActive(true);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    int i = Random.Range(0, poolNames.Length);
                    Pool pCheck = GetPool(poolNames[i]);
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        SmartBuffer(poolNames[i], tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandom: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
        }
        #endregion

        #region SPAWN RANDOM REFERENCE
        /// <summary>
        /// Spawns random object with given pool name and default position; returns a reference.
        /// </summary>
        /// <returns>The random reference.</returns>
        /// <param name="poolName">Pool name.</param>
        /// <param name="position">Position.</param>
        public GameObject SpawnRandomReference(string poolName)
        {
            GameObject tPoolObj = null;
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                List<GameObject> inactive = FindInactive(pCheck.pool);
                if (inactive.Count > 0)
                {
                    tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        tPoolObj = SmartBufferReference(poolName, tObj);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandomReference: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("SpawnRandom: Pool -> '" + poolName + "' not found. Please verify spelling");
            }
            return tPoolObj;
        }
        /// <summary>
        /// Spawns random object with given pool name and position; returns a reference.
        /// </summary>
        /// <returns>The random reference.</returns>
        /// <param name="poolName">Pool name.</param>
        /// <param name="position">Position.</param>
        public GameObject SpawnRandomReference(string poolName, Vector3 position)
        {
            GameObject tPoolObj = null;
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                List<GameObject> inactive = FindInactive(pCheck.pool);
                if (inactive.Count > 0)
                {
                    tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        tPoolObj = SmartBufferReference(poolName, tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandomReference: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("SpawnRandom: Pool -> '" + poolName + "' not found. Please verify spelling");
            }
            return tPoolObj;
        }
        /// <summary>
        /// Spawns random object from given list of pool names at given position; returns a reference.
        /// </summary>
        /// <returns>The random reference.</returns>
        /// <param name="poolNames">List of pool names</param>
        /// <param name="position">Position.</param>
        public GameObject SpawnRandomReference(List<string> poolNames, Vector3 position)
        {
            GameObject tPoolObj = null;
            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> inactive = FindInactive(MergedPool(poolNames));
                if (inactive.Count > 0)
                {
                    tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    int i = Random.Range(0, poolNames.Count);
                    Pool pCheck = GetPool(poolNames[i]);
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        tPoolObj = SmartBufferReference(poolNames[i], tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandomReference: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            return tPoolObj;
        }
        /// <summary>
        /// Spawns random object from given array of pool names at given position; returns a reference.
        /// </summary>
        /// <returns>The random reference.</returns>
        /// <param name="poolNames">List of pool names</param>
        /// <param name="position">Position.</param>
        public GameObject SpawnRandomReference(string[] poolNames, Vector3 position)
        {
            GameObject tPoolObj = null;
            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> inactive = FindInactive(MergedPool(poolNames));
                if (inactive.Count > 0)
                {
                    tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    int i = Random.Range(0, poolNames.Length);
                    Pool pCheck = GetPool(poolNames[i]);
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        tPoolObj = SmartBufferReference(poolNames[i], tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandomReference: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            return tPoolObj;
        }

        /// <summary>
        /// Spawns a random object with given pool name, position, and rotation; returns a reference.
        /// </summary>
        /// <returns>The random reference.</returns>
        /// <param name="poolName">Pool name.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        public GameObject SpawnRandomReference(string poolName, Vector3 position, Quaternion rotation)
        {
            GameObject tPoolObj = null;
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                List<GameObject> inactive = FindInactive(pCheck.pool);
                if (inactive.Count > 0)
                {
                    tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.transform.rotation = rotation;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        tPoolObj = SmartBufferReference(poolName, tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandomReference: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("SpawnRandom: Pool -> '" + poolName + "' not found. Please verify spelling");
            }
            return tPoolObj;
        }
        /// <summary>
        /// Spawns a random object from one of the given pool names at the given position and rotation; returns a reference.
        /// </summary>
        /// <returns>The random reference.</returns>
        /// <param name="poolName">Pool names.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        public GameObject SpawnRandomReference(List<string> poolNames, Vector3 position, Quaternion rotation)
        {
            GameObject tPoolObj = null;
            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> inactive = FindInactive(MergedPool(poolNames));
                if (inactive.Count > 0)
                {
                    tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.transform.rotation = rotation;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");
                    }
                }
                else
                {
                    int i = Random.Range(0, poolNames.Count);
                    Pool pCheck = GetPool(poolNames[i]);
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        tPoolObj = SmartBufferReference(poolNames[i], tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandomReference: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            return tPoolObj;
        }
        /// <summary>
        /// Spawns a random object from one of the given pool names at the given position and rotation; returns a reference.
        /// </summary>
        /// <returns>The random reference.</returns>
        /// <param name="poolName">Pool names.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        public GameObject SpawnRandomReference(string[] poolNames, Vector3 position, Quaternion rotation)
        {
            GameObject tPoolObj = null;
            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> inactive = FindInactive(MergedPool(poolNames));
                if (inactive.Count > 0)
                {
                    tPoolObj = inactive[Random.Range(0, inactive.Count)];
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        tPoolObj.transform.position = position;
                        tPoolObj.transform.rotation = rotation;
                        tPoolObj.SetActive(true);
                        CleanupObject(tPoolObj);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SpawnRandom: Object -> '" + tPoolObj.name.Split(' ')[0] + "' spawned successfully.");

                    }
                }
                else
                {
                    int i = Random.Range(0, poolNames.Length);
                    Pool pCheck = GetPool(poolNames[i]);
                    GameObject tObj = pCheck.uniquePool[Random.Range(0, pCheck.uniquePool.Count)];
                    if (smartBufferMode > 0 && tObj != null)
                    {
                        tPoolObj = SmartBufferReference(poolNames[i], tObj, position);
                    }
                    else
                    {
                        if (Application.isEditor && debugMode > 0)
                        {
                            Debug.Log("SpawnRandomReference: '" + tObj.name + "' found but not loaded. Please preload your pool or turn on smart buffering.");
                        }
                    }
                }
            }
            return tPoolObj;
        }
        #endregion

        #region DESPAWN
        /// <summary>
        /// Despawn the specified GameObject.
        /// </summary>
        /// <param name="g">The game object to despawn.</param>
        public void Despawn(GameObject g)
        {
            if (g != null)
            {
                if (g.activeSelf)
                {
                    PooledObject pObject = g.GetComponent<PooledObject>();
                    if (pObject != null)
                    {
                        if (g.transform.parent != pObject._parentTransform && pObject.hasLoaded)
                            pObject.Reparent();
                    }
                    if (g.transform.childCount > 0)
                    {
                        for (int i = 0; i < g.transform.childCount; i++)
                        {
                            if (g.transform.GetChild(i).gameObject.activeInHierarchy)
                            {
                                PooledObject tPooledObject = g.transform.GetChild(i).gameObject.GetComponent<PooledObject>();
                                if (tPooledObject != null)
                                {
                                    tPooledObject.Reparent();
                                    tPooledObject.Despawn();
                                }
                            }
                        }
                    }
                    if (Application.isEditor && debugMode > 1)
                        Debug.Log("Despawn: Object -> '" + g.name + "' despawned successfully.");
                    g.SetActive(false);
                    PoolMasterEvents.EventDespawnObject(g);
                }
            }
            else
            {
                if (Application.isEditor && debugMode > 0)
                    Debug.LogWarning("Despawn: Object -> '" + g.name + "' not found! Please check spelling.");
            }
        }

        /// <summary>
        /// Despawn the specified GameObject with a wait time.
        /// </summary>
        /// <param name="g">The game object to despawn.</param>
        public void Despawn(GameObject g, float waitTime)
        {
            if (g != null)
            {
                PooledObject pObj = g.GetComponent<PooledObject>();
                if (pObj != null)
                    pObj.Despawn(waitTime);
            }
        }

        /// <summary>
        /// Despawn the specified GameObject.
        /// </summary>
        /// <param name="g">The game object to despawn.</param>
        public void Despawn(GameObject g, bool ignoreDebug)
        {
            if (g != null)
            {
                if (g.activeSelf)
                {
                    PooledObject pObject = g.GetComponent<PooledObject>();
                    if (pObject != null)
                    {
                        if (g.transform.parent != pObject._parentTransform && pObject.hasLoaded)
                            pObject.Reparent();
                    }
                    if (g.transform.childCount > 0)
                    {
                        for (int i = 0; i < g.transform.childCount; i++)
                        {
                            if (g.transform.GetChild(i).gameObject.activeInHierarchy)
                            {
                                PooledObject tPooledObject = g.transform.GetChild(i).gameObject.GetComponent<PooledObject>();
                                if (tPooledObject != null)
                                {
                                    tPooledObject.Reparent();
                                    tPooledObject.Despawn();
                                }
                            }
                        }
                    }
                    if (Application.isEditor && debugMode > 1 && !ignoreDebug)
                        Debug.Log("Despawn: Object -> '" + g.name + "' despawned successfully.");
                    g.SetActive(false);
                    PoolMasterEvents.EventDespawnObject(g);
                }
            }
            else
            {
                if (Application.isEditor && debugMode > 0 && !ignoreDebug)
                    Debug.LogWarning("Despawn: Object -> '" + g.name + "' not found! Please check spelling.");
            }
        }

        /// <summary>
        /// Despawn the specified poolName.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        public void Despawn(string poolName)
        {
            Transform parentTransform = transform.FindChild(poolName);
            if (parentTransform != null)
            {
                GameObject parent = parentTransform.gameObject;
                PoolMasterEvents.EventDespawnPool(poolName);
                if (Application.isEditor && debugMode > 1)
                    Debug.Log("Despawn: Pool -> '" + poolName + "' despawned successfully.");

                foreach (Transform t in parent.transform)
                    Despawn(t.gameObject, true);
            }
            else if (debugMode > 0 && Application.isEditor)
            {
                Debug.LogWarning("Despawn: Pool -> '" + poolName + "' not found! Please verify spelling.");
            }

        }
        /// <summary>
        /// Despawn the specified poolNames.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        public void Despawn(string[] poolNames)
        {
            foreach (string s in poolNames)
            {
                Despawn(s);
            }
        }
        /// <summary>
        /// Despawn the specified poolNames.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        public void Despawn(List<string> poolNames)
        {
            foreach (string s in poolNames)
            {
                Despawn(s);
            }
        }
        #endregion

        #region DESTROY
        /// <summary>
        /// Destroy the specified g.
        /// </summary>
        /// <param name="g">The green component.</param>
        public void Destroy(GameObject g)
        {
            if (Application.isEditor && debugMode > 1)
                Debug.Log("Destroy: Object -> '" + g.name + "' destroyed successfully.");
            GameObject.Destroy(g);
            PoolMasterEvents.EventDestroyObject(g);
            if (smartBufferMode > 0 && smartBufferCount > 0)
                smartBufferCount--;
        }
        /// <summary>
        /// Destroy the specified poolName.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        public void Destroy(string poolName)
        {
            Pool pCheck = GetPool(poolName);
            if (pCheck != null)
            {
                if (Application.isEditor && debugMode > 1)
                    Debug.LogWarning("Destroy: Pool -> '" + poolName + "' destroyed successfully.");
                pCheck.hasLoaded = false;
                pCheck.pool.Clear();
                Transform parentTransform = transform.FindChild(pCheck.name);
                if (parentTransform != null)
                {
                    GameObject parent = parentTransform.gameObject;
                    if (parent != null)
                    {
                        for (int i = 0; i < parent.transform.childCount; i++)
                        {
                            if (smartBufferMode > 0 && smartBufferCount > 0)
                                smartBufferCount--;
                        }
                        GameObject.Destroy(parent);
                        PoolMasterEvents.EventDestroyPool(poolName);
                    }
                }
            }
            else
            {
                if (Application.isEditor && debugMode > 0)
                    Debug.LogWarning("Destroy: Pool -> '" + poolName + "' not found! Please verify spelling.");
            }


        }
        /// <summary>
        /// Destroy the specified poolNames.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        public void Destroy(string[] poolNames)
        {
            foreach (string s in poolNames)
            {
                Instance.Destroy(s);
            }
        }
        /// <summary>
        /// Destroy the specified poolNames.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        public void Destroy(List<string> poolNames)
        {
            foreach (string s in poolNames)
            {
                Instance.Destroy(s);
            }
        }


        #endregion

        #region SMART BUFFER
        /// <summary>
        /// Smarts the buffer.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="obj">Object.</param>
        private void SmartBuffer(string poolName, GameObject obj)
        {
            if (smartBufferCount < smartBufferMax)
            {
                Pool pCheck = GetPool(poolName);
                if (pCheck != null)
                {
                    if (!pCheck.hasLoaded)
                        Preload(pCheck.name);
                    GameObject tPoolObj = LastInstanceOf(pCheck, obj);
                    if (tPoolObj != null)
                    {
                        int lastIndex = ObjectCount(pCheck, obj);
                        GameObject g = (GameObject)Instantiate(obj);
                        g.name = g.name.Replace(' ', '_');
                        g.name = g.name.Split('(')[0] + " - " + (lastIndex).ToString();
                        g.transform.parent = this.transform.FindChild(poolName);
                        PooledObject pObject = g.AddComponent<PooledObject>();
                        pObject.Define(this, g.transform.parent, pools.IndexOf(pCheck), tPoolObj.GetComponent<PooledObject>().ItemIndex);
                        pools[pObject.PoolIndex].bufferAmount[pObject.ItemIndex]++;
                        pCheck.pool.Add(g);
                        smartBufferCount++;
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SmartBuffer: Object -> '" + g.name + "' successfully added to  Pool -> '" + pCheck.name + "'");
                        if (g.GetComponent<AudioSource>() != null)
                        {
                            if (g.GetComponent<PooledAudio>() == null)
                            {
                                g.AddComponent<PooledAudio>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        if (g.GetComponent<ParticleSystem>() != null)
                        {
                            if (g.GetComponent<PooledParticle>() == null)
                            {
                                g.AddComponent<PooledParticle>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        CleanupObject(g);
                        pObject.hasLoaded = true;
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(g);
                    }
                }
                else
                {
                    if (Application.isEditor && debugMode > 0)
                        Debug.LogWarning("SmartBuffer: Maximum buffer reached. Adjust buffer levels");
                }
            }
        }

        /// <summary>
        /// Smarts the buffer.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="obj">Object.</param>
        /// <param name="position">Position.</param>
        private void SmartBuffer(string poolName, GameObject obj, Vector3 position)
        {
            if (smartBufferCount < smartBufferMax)
            {
                Pool pCheck = GetPool(poolName);

                if (pCheck != null)
                {
                    if (!pCheck.hasLoaded)
                        Preload(pCheck.name);
                    GameObject tPoolObj = LastInstanceOf(pCheck, obj);
                    if (tPoolObj != null)
                    {
                        int lastIndex = ObjectCount(pCheck, obj);
                        GameObject g = (GameObject)Instantiate(obj);
                        g.transform.position = position;
                        g.name = g.name.Replace(' ', '_');
                        g.name = g.name.Split('(')[0] + " - " + (lastIndex).ToString();
                        g.transform.parent = this.transform.FindChild(poolName);
                        PooledObject pObject = g.AddComponent<PooledObject>();
                        pObject.Define(this, g.transform.parent, pools.IndexOf(pCheck), tPoolObj.GetComponent<PooledObject>().ItemIndex);
                        pools[pObject.PoolIndex].bufferAmount[pObject.ItemIndex]++;
                        pCheck.pool.Add(g);
                        smartBufferCount++;
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SmartBuffer: Object -> '" + g.name + "' successfully added to  Pool -> '" + pCheck.name + "'");
                        if (g.GetComponent<AudioSource>() != null)
                        {
                            if (g.GetComponent<PooledAudio>() == null)
                            {
                                g.AddComponent<PooledAudio>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        if (g.GetComponent<ParticleSystem>() != null)
                        {
                            if (g.GetComponent<PooledParticle>() == null)
                            {
                                g.AddComponent<PooledParticle>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        CleanupObject(g);
                        pObject.hasLoaded = true;
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(g);
                    }
                }
            }
            else
            {
                if (Application.isEditor && debugMode > 0)
                    Debug.LogWarning("SmartBuffer: Maximum buffer reached. Adjust buffer levels");
            }
        }

        /// <summary>
        /// Smarts the buffer.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="obj">Object.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        private void SmartBuffer(string poolName, GameObject obj, Vector3 position, Quaternion rotation)
        {
            if (smartBufferCount < smartBufferMax)
            {
                Pool pCheck = GetPool(poolName);
                if (pCheck != null)
                {
                    if (!pCheck.hasLoaded)
                        Preload(pCheck.name);
                    GameObject tPoolObj = LastInstanceOf(pCheck, obj);
                    if (tPoolObj != null)
                    {
                        int lastIndex = ObjectCount(pCheck, obj);
                        GameObject g = (GameObject)Instantiate(obj, position, rotation);
                        g.name = g.name.Replace(' ', '_');
                        g.name = g.name.Split('(')[0] + " - " + (lastIndex).ToString();
                        g.transform.parent = this.transform.FindChild(poolName);
                        PooledObject pObject = g.AddComponent<PooledObject>();
                        pObject.Define(this, g.transform.parent, pools.IndexOf(pCheck), tPoolObj.GetComponent<PooledObject>().ItemIndex);
                        pools[pObject.PoolIndex].bufferAmount[pObject.ItemIndex]++;
                        pCheck.pool.Add(g);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SmartBuffer: Object -> '" + g.name + "' successfully added to  Pool -> '" + pCheck.name + "'");
                        if (g.GetComponent<AudioSource>() != null)
                        {
                            if (g.GetComponent<PooledAudio>() == null)
                            {
                                g.AddComponent<PooledAudio>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        if (g.GetComponent<ParticleSystem>() != null)
                        {
                            if (g.GetComponent<PooledParticle>() == null)
                            {
                                g.AddComponent<PooledParticle>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        CleanupObject(g);
                        pObject.hasLoaded = true;
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(g);
                    }
                }
            }
            else
            {
                if (Application.isEditor && debugMode > 0)
                    Debug.LogWarning("SmartBuffer: Maximum buffer reached. Adjust buffer levels");
            }
        }

        /// <summary>
        /// Smarts the buffer.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="obj">Object.</param>
        private GameObject SmartBufferReference(string poolName, GameObject obj)
        {

            GameObject g = null;
            if (smartBufferCount < smartBufferMax)
            {
                Pool pCheck = GetPool(poolName);
                if (pCheck != null)
                {
                    if (!pCheck.hasLoaded)
                        Preload(pCheck.name);
                    GameObject tPoolObj = LastInstanceOf(pCheck, obj);
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        int lastIndex = ObjectCount(pCheck, obj);
                        g = (GameObject)Instantiate(obj);
                        g.name = g.name.Replace(' ', '_');
                        g.name = g.name.Split('(')[0] + " - " + (lastIndex).ToString();
                        g.transform.parent = this.transform.FindChild(poolName);
                        PooledObject pObject = g.AddComponent<PooledObject>();
                        pObject.Define(this, g.transform.parent, pools.IndexOf(pCheck), tPoolObj.GetComponent<PooledObject>().ItemIndex);
                        pools[pObject.PoolIndex].bufferAmount[pObject.ItemIndex]++;
                        pCheck.pool.Add(g);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SmartBufferReference: Object -> '" + g.name + "' successfully added to  Pool -> '" + pCheck.name + "'");
                        if (g.GetComponent<AudioSource>() != null)
                        {
                            if (g.GetComponent<PooledAudio>() == null)
                            {
                                g.AddComponent<PooledAudio>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        if (g.GetComponent<ParticleSystem>() != null)
                        {
                            if (g.GetComponent<PooledParticle>() == null)
                            {
                                g.AddComponent<PooledParticle>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        CleanupObject(g);
                        pObject.hasLoaded = true;
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(g);
                    }
                }
            }
            else
            {
                if (Application.isEditor && debugMode > 0)
                    Debug.LogWarning("SmartBufferReference: Maximum buffer reached. Adjust buffer levels");
            }
            return g;
        }

        /// <summary>
        /// Smarts the buffer.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="obj">Object.</param>
        /// <param name="position">Position.</param>
        private GameObject SmartBufferReference(string poolName, GameObject obj, Vector3 position)
        {
            GameObject g = null;
            if (smartBufferCount < smartBufferMax)
            {
                Pool pCheck = GetPool(poolName);
                if (pCheck != null)
                {
                    if (!pCheck.hasLoaded)
                        Preload(pCheck.name);
                    GameObject tPoolObj = LastInstanceOf(pCheck, obj);
                    if (tPoolObj != null)
                    {
                        int lastIndex = ObjectCount(pCheck, obj);
                        g = (GameObject)Instantiate(obj, position, Quaternion.identity);
                        g.name = g.name.Replace(' ', '_');
                        g.name = g.name.Split('(')[0] + " - " + (lastIndex).ToString();
                        g.transform.parent = this.transform.FindChild(poolName);
                        PooledObject pObject = g.AddComponent<PooledObject>();
                        pObject.Define(this, g.transform.parent, pools.IndexOf(pCheck), tPoolObj.GetComponent<PooledObject>().ItemIndex);
                        pools[pObject.PoolIndex].bufferAmount[pObject.ItemIndex]++;
                        pCheck.pool.Add(g);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SmartBufferReference: Object -> '" + g.name + "' successfully added to  Pool -> '" + pCheck.name + "'");
                        if (g.GetComponent<AudioSource>() != null)
                        {
                            if (g.GetComponent<PooledAudio>() == null)
                            {
                                g.AddComponent<PooledAudio>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        if (g.GetComponent<ParticleSystem>() != null)
                        {
                            if (g.GetComponent<PooledParticle>() == null)
                            {
                                g.AddComponent<PooledParticle>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        CleanupObject(g);
                        pObject.hasLoaded = true;
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(g);
                    }
                }
            }
            else
            {
                if (Application.isEditor && debugMode > 0)
                    Debug.LogWarning("SmartBufferReference: Maximum buffer reached. Adjust buffer levels");
            }
            return g;
        }

        /// <summary>
        /// Smarts the buffer.
        /// </summary>
        /// <param name="poolName">Pool name.</param>
        /// <param name="obj">Object.</param>
        /// <param name="position">Position.</param>
        private GameObject SmartBufferReference(string poolName, GameObject obj, Vector3 position, Quaternion rotation)
        {
            GameObject g = null;
            if (smartBufferCount < smartBufferMax)
            {
                Pool pCheck = GetPool(poolName);
                if (pCheck != null)
                {
                    if (!pCheck.hasLoaded)
                        Preload(pCheck.name);
                    GameObject tPoolObj = LastInstanceOf(pCheck, obj);
                    if (tPoolObj != null)
                    {
                        if (eventMode > 0)
                            PoolMasterEvents.EventSpawn(tPoolObj);
                        int lastIndex = ObjectCount(pCheck, obj);
                        g = (GameObject)Instantiate(obj, position, rotation);
                        g.name = g.name.Replace(' ', '_');
                        g.name = g.name.Split('(')[0] + " - " + (lastIndex).ToString();
                        g.transform.parent = this.transform.FindChild(poolName);
                        PooledObject pObject = g.AddComponent<PooledObject>();
                        pObject.Define(this, g.transform.parent, pools.IndexOf(pCheck), tPoolObj.GetComponent<PooledObject>().ItemIndex);
                        pools[pObject.PoolIndex].bufferAmount[pObject.ItemIndex]++;
                        pCheck.pool.Add(g);
                        if (Application.isEditor && debugMode > 1)
                            Debug.Log("SmartBufferReference: Object -> '" + g.name + "' successfully added to  Pool -> '" + pCheck.name + "'");
                        if (g.GetComponent<AudioSource>() != null)
                        {
                            if (g.GetComponent<PooledAudio>() == null)
                            {
                                g.AddComponent<PooledAudio>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        if (g.GetComponent<ParticleSystem>() != null)
                        {
                            if (g.GetComponent<PooledParticle>() == null)
                            {
                                g.AddComponent<PooledParticle>();
                            }
                            g.SetActive(false);
                            g.SetActive(true);
                        }
                        CleanupObject(g);
                        pObject.hasLoaded = true;
                    }
                }
                else
                {
                    if (Application.isEditor && debugMode > 0)
                        Debug.LogWarning("SmartBufferReference: Maximum buffer reached. Adjust buffer levels");
                }
            }
            return g;
        }
        #endregion

        #region GET POOL
        /// <summary>
        /// Gets a reference to a pool with a given name.
        /// </summary>
        /// <returns>The pool.</returns>
        /// <param name="name">Name.</param>
        public Pool GetPool(string name)
        {
            List<Pool> tempList = new List<Pool>();
            for (int i = 0; i < pools.Count; i++)
            {
                if (pools[i].name == name)
                {
                    tempList.Add(pools[i]);
                }
            }
            if (tempList.Count > 0)
            {
                return tempList[tempList.Count - 1];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a reference to a pool with a given index.
        /// </summary>
        /// <returns>The pool.</returns>
        /// <param name="index">Index.</param>
        public Pool GetPool(int index)
        {
            if (index < this.pools.Count)
                return pools[index];
            else
                return null;
        }
        #endregion

        #region CHECK POOL EXISTS
        /// <summary>
        /// Checks the pool exists.
        /// </summary>
        /// <returns><c>true</c>, if pool exists was checked, <c>false</c> otherwise.</returns>
        /// <param name="poolName">Pool name.</param>
        private bool CheckPoolExists(string poolName)
        {
            if (GetPool(poolName) != null)
            {
                return true;
            }
            else
            {
                if (debugMode > 0 && Application.isEditor)
                    Debug.LogWarning("CheckPoolExists: Pool -> '" + poolName + "' not found. Please verify spelling");
                return false;
            }
        }
        /// <summary>
        /// Checks the pools exist.
        /// </summary>
        /// <returns><c>true</c>, if pools exist, <c>false</c> otherwise.</returns>
        /// <param name="poolNames">Pool names.</param>
        private bool CheckPoolsExist(List<string> poolNames)
        {
            bool b = true;
            foreach (string name in poolNames)
            {
                if (!CheckPoolExists(name))
                {
                    b = false;
                }
            }
            return b;
        }
        /// <summary>
        /// Checks the pools exist.
        /// </summary>
        /// <returns><c>true</c>, if pools exist, <c>false</c> otherwise.</returns>
        /// <param name="poolNames">Pool names.</param>
        private bool CheckPoolsExist(string[] poolNames)
        {
            bool b = true;
            foreach (string name in poolNames)
            {
                if (!CheckPoolExists(name))
                {
                    b = false;
                }
            }
            return b;
        }
        #endregion

        #region MERGED POOL
        /// <summary>
        /// Returns merged pool
        /// </summary>
        /// <returns>The pool.</returns>
        /// <param name="poolNames">Pool names.</param>
        private List<GameObject> MergedPool(List<string> poolNames)
        {
            if (CheckPoolsExist(poolNames))
            {
                List<GameObject> mergedList = new List<GameObject>();
                foreach (string s in poolNames)
                {
                    Pool p = GetPool(s);
                    mergedList.AddRange(p.pool);
                }
                return mergedList;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Merges the pool
        /// </summary>
        /// <returns>The pool.</returns>
        /// <param name="poolNames">Pool names.</param>
        private List<GameObject> MergedPool(string[] poolNames)
        {
            List<GameObject> mergedList = new List<GameObject>();
            if (CheckPoolsExist(poolNames))
            {
                foreach (string s in poolNames)
                {
                    Pool p = GetPool(s);
                    mergedList.AddRange(p.pool);
                }
            }
            else
            {
                return null;
            }
            return mergedList;
        }
        #endregion

        #region CLEANUP 
        public void CleanupObject(GameObject g)
        {
            if (g.activeInHierarchy)
            {
                PooledObject pObject = g.GetComponent<PooledObject>();
                if (pObject != null)
                {
                    if (g.transform.parent != pObject._parentTransform && pObject.hasLoaded)
                        pObject.Reparent();
                }
                if (g.transform.childCount > 0)
                {
                    for (int i = 0; i < g.transform.childCount; i++)
                    {
                        if (g.transform.GetChild(i).gameObject.activeInHierarchy)
                        {
                            PooledObject tPooledObject = g.transform.GetChild(i).gameObject.GetComponent<PooledObject>();
                            if (tPooledObject != null)
                            {
                                tPooledObject.Reparent();
                            }
                        }
                    }
                }
            }

        }
        #endregion

        #region 64-BIT SUPPORT
        private GameObject GetUniqueObject(Pool p, string name)
        {
            List<GameObject> tempList = new List<GameObject>();
            for (int i = 0; i < p.uniquePool.Count; i++)
            {
                if (p.uniquePool[i].name == name)
                    tempList.Add(p.uniquePool[i]);
            }
            if (tempList.Count > 0)
            {
                return tempList[tempList.Count - 1];
            }
            else
            {
                return null;
            }
        }

        private GameObject LastInstanceOf(Pool p, GameObject reference)
        {
            List<GameObject> tempList = new List<GameObject>();
            for (int i = 0; i < p.pool.Count; i++)
            {
                if (p.pool[i].name.Split(' ')[0] == reference.name.Replace(' ', '_'))
                {
                    tempList.Add(p.pool[i]);
                }
            }
            if (tempList.Count != 0)
            {
                return tempList[tempList.Count - 1];
            }
            else
            {
                return null;
            }
        }

        private GameObject GetInactiveObject(Pool p, GameObject reference)
        {
            List<GameObject> activeList = new List<GameObject>();
            List<GameObject> tempList = new List<GameObject>();
            for (int i = 0; i < p.pool.Count; i++)
            {
                if (!p.pool[i].activeSelf)
                {
                    activeList.Add(p.pool[i]);
                }
            }
            if (activeList.Count > 0)
            {
                for (int i = 0; i < activeList.Count; i++)
                {
                    if (activeList[i].name.Split(' ')[0] == reference.name.Replace(' ', '_'))
                    {
                        tempList.Add(activeList[i]);
                    }
                }
                if (tempList.Count > 0)
                    return tempList[0];
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        private List<GameObject> FindInactive(List<GameObject> objectList)
        {
            List<GameObject> tList = new List<GameObject>();
            for (int i = 0; i < objectList.Count; i++)
            {
                if (!objectList[i].activeSelf)
                    tList.Add(objectList[i]);
            }
            return tList;

        }

        private int ObjectCount(Pool p, GameObject reference)
        {
            int count = 0;
            for (int i = 0; i < p.pool.Count; i++)
            {
                if (p.pool[i].name.Split(' ')[0] == reference.name.Replace(' ', '_'))
                {
                    count++;
                }
            }
            return count;
        }
        #endregion

    }

}
