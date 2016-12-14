using UnityEngine;
using lpesign;

using hamsterbyte.PoolMaster;

namespace Game
{

    [RequireComponent(typeof(ObjectPool))]
    public class GamePool : SingletonSimple<GamePool>
    {

        public bool IsReady { get; private set; }

        ObjectPool _pool;
        override protected void Awake()
        {
            base.Awake();


            _pool = PoolMaster.instance;

            //doc
            //doc http://www.hamsterbyte.com/category/support/poolmaster/
            PoolMasterEvents.onPreloadPool += OnPreloadPool;
            PoolMasterEvents.onSpawn += OnSpawn;
            PoolMasterEvents.onDespawnObject += OnDespawnObject;
            PoolMasterEvents.onDespawnPool += OnDespawnPool;
            PoolMasterEvents.onDestroyObject += OnDestroyObject;
            PoolMasterEvents.onDestroyPool += OnDestroyPool;
            PoolMasterEvents.onPlayAudio += OnPlayAudio;
        }

        override protected void OnDestroy()
        {
            base.OnDestroy();

            PoolMasterEvents.onPreloadPool -= OnPreloadPool;
            PoolMasterEvents.onSpawn -= OnSpawn;
            PoolMasterEvents.onDespawnObject -= OnDespawnObject;
            PoolMasterEvents.onDespawnPool -= OnDespawnPool;
            PoolMasterEvents.onDestroyObject -= OnDestroyObject;
            PoolMasterEvents.onDestroyPool -= OnDestroyPool;
            PoolMasterEvents.onPlayAudio -= OnPlayAudio;
        }

        void Start()
        {
            _pool.Preload("Symbols");
        }

        void OnPreloadPool(string poolName)
        {
            if (poolName == "Symbols") IsReady = true;
        }

        public Symbol SpawnSymbol(string objName)
        {
            GameObject go = _pool.SpawnReference("Symbols", objName);

            if (go == null) return null;
            else return go.GetComponent<Symbol>();
        }

        public void DespawnSymbol(Symbol symbol)
        {
            if (symbol == null) return;
            _pool.Despawn(symbol.gameObject);
        }


        void OnSpawn(GameObject g)
        {
            //Debug.Log("OnSpawn : " + g);
        }

        void OnDespawnObject(GameObject g)
        {
            //Debug.Log("OnDespawnObject: " + g);
        }

        void OnDespawnPool(string poolName)
        {
            Debug.Log("OnDespawnPool : " + poolName);
        }

        void OnDestroyObject(GameObject g)
        {
            Debug.Log("OnDestroyObject : " + g);
        }

        void OnDestroyPool(string poolName)
        {
            Debug.Log("OnDestroyPool : " + poolName);
        }

        void OnPlayAudio(string clipName, GameObject objReference)
        {
            Debug.Log("OnPlayAudio : " + clipName + " : " + objReference);
        }
    }
}
