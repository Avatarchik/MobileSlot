using UnityEngine;
using lpesign;

using hamsterbyte.PoolMaster;

namespace Game
{

    [RequireComponent(typeof(ObjectPool))]
    public class GamePool : SingletonSimple<GamePool>
    {
        [HideInInspector]
        static private ObjectPool _pool;

        private const string POOL_SYMBOL = "SymbolPool";
        static public bool IsReady { get; private set; }

        override protected void Awake()
        {
            base.Awake();

            Show();

            _pool = Object.FindObjectOfType<ObjectPool>();
            if (_pool == null) _pool = gameObject.AddComponent<ObjectPool>();

            _pool.hideMode = 0;//off,selective,all
            _pool.eventMode = 1;//off,on
            _pool.preloadMode = 0;//non,selective,all
            _pool.persistenceMode = 0;//off,selective,all
            _pool.smartBufferMode = 1;//off,on
            _pool.smartBufferMax = 255;
        }

        override protected void OnDestroy()
        {
            base.OnDestroy();

            PoolMasterEvents.onPreloadPool -= OnPreloadPool;
        }


        static public void Initialize(SlotConfig config)
        {
            CreatePools(config);
            Load();
        }

        static private void CreatePools(SlotConfig config)
        {
            var symbolPool = new Pool(POOL_SYMBOL);
            var machineList = config.MachineList;
            for (var i = 0; i < machineList.Count; ++i)
            {
                var machine = machineList[i];
                var list = machine.SymbolList;
                for (var j = 0; j < list.Count; ++j)
                {
                    var symbolDefine = list[j];
                    symbolPool.uniquePool.Add(symbolDefine.prefab.gameObject);
                    symbolPool.bufferAmount.Add(symbolDefine.buffer);
                }
            }
            _pool.pools.Add(symbolPool);
        }

        static private void Load()
        {
            PoolMasterEvents.onPreloadPool += OnPreloadPool;

            _pool.Preload(POOL_SYMBOL);
        }

        static private void OnPreloadPool(string poolName)
        {
            if (poolName == POOL_SYMBOL)
            {
                IsReady = true;
                PoolMasterEvents.onPreloadPool -= OnPreloadPool;
            }
        }

        static public Symbol SpawnSymbol(string objName)
        {
            GameObject go = _pool.SpawnReference(POOL_SYMBOL, objName);

            if (go == null) return null;
            else return go.GetComponent<Symbol>();
        }

        static public void DespawnSymbol(Symbol symbol)
        {
            if (symbol == null) return;
            _pool.Despawn(symbol.gameObject);
        }
    }
}
