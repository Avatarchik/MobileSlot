using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace hamsterbyte.PoolMaster {

	[System.Serializable]
	[RequireComponent(typeof(ObjectPool))]
	public class PoolMasterSpawnPoints : MonoBehaviour {
		public string[] HandleModes = new string[] {"None", "Selective", "All"};
		public int handleMode = 2;
		public List<SpawnGroup> spawnGroups;

		public PoolMasterSpawnPoints() {
			spawnGroups = new List<SpawnGroup>();
		}

		public void AddGroup(string name){
			if(spawnGroups != null){
				spawnGroups.Add(new SpawnGroup());
			} else {
				spawnGroups = new List<SpawnGroup>();
				spawnGroups.Add(new SpawnGroup());
			}
		}

		public void RemoveGroup(int index){
			spawnGroups.RemoveAt(index);
		}

		public void ClearGroups(){
			spawnGroups.Clear();
		}
	}

	[System.Serializable]
	public class SpawnGroup {
		public string name;
		public List<Vector3> spawnPoints;
		public Color handleColor;
		public string[] HandleShapes = new string[] {"Sphere", "Cube", "Cone", "Diamond"};
		public int handleShape;
		public bool showHandles = true;

		public SpawnGroup () {
			name = "New Spawn Group";
			spawnPoints = new List<Vector3>();
			handleColor = Color.white;
		}

		public SpawnGroup (string groupName) {
			name = groupName;
			spawnPoints = new List<Vector3>();
			handleColor = Color.white;
		}

		public void AddPoint() {
			if(spawnPoints == null)
				spawnPoints = new List<Vector3>();
			spawnPoints.Add(Vector3.zero);
		}

		public void AddPoint(Vector3 point) {
			if(spawnPoints == null)
				spawnPoints = new List<Vector3>();
			spawnPoints.Add(point);
		}

		public void RemovePoint(int index){
			try{
				spawnPoints.RemoveAt(index);
			} catch(Exception e) {
				Debug.Log(e.Message);
			}
		}

		public void ClearPoints(){
			try {
				spawnPoints.Clear();
			} catch (Exception e) {
				Debug.Log(e.Message);
			}
		}

	}
}
