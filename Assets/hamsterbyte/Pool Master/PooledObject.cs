using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace hamsterbyte.PoolMaster {
	public class PooledObject : MonoBehaviour {

		private ObjectPool _objectPool;
		public Transform _parentTransform;
		private int _poolIndex;
		private int _itemIndex;
		private bool _defined;
		public bool hasLoaded;

		public int ItemIndex {get{return _itemIndex;}}
		public int PoolIndex {get{return _poolIndex;}}

		void OnEnable(){
			if(_defined)
				ModifyActiveCount(1);
		}

		void OnDisable(){
			if(_defined){
				ModifyActiveCount(-1);
			}
		}

		public void Define(ObjectPool oPool, Transform parent, int index, int iIndex){
			_objectPool = oPool;
			_parentTransform = parent;
			_poolIndex = index;
			_itemIndex = iIndex;
			_defined = true;
		}

		public void Reparent(){
			this.transform.parent = _parentTransform;
		}

		public void ModifyActiveCount(int amt){
			_objectPool.totalActive += amt;
			_objectPool.pools[_poolIndex].totalActive += amt;
			_objectPool.pools[_poolIndex].activeCount[_itemIndex] += amt;
			_objectPool.totalActive = Mathf.Clamp(_objectPool.totalActive, 0, int.MaxValue);
			_objectPool.pools[_poolIndex].totalActive = Mathf.Clamp(_objectPool.pools[_poolIndex].totalActive, 0, int.MaxValue);
			_objectPool.pools[_poolIndex].activeCount[_itemIndex] = Mathf.Clamp(_objectPool.pools[_poolIndex].activeCount[_itemIndex], 0, int.MaxValue);
		}

		public void Despawn(){
			_objectPool.Despawn(this.gameObject);
		}

		public void Despawn(float waitTime) {
			StartCoroutine("WaitAndDespawn", waitTime);
		}

		public IEnumerator WaitAndDespawn (float waitTime){
			if(waitTime < 0)
				waitTime = 0;
			yield return new WaitForSeconds(waitTime);
			this.Despawn();
		}
	}
}