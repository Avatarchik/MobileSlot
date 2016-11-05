using UnityEngine;
using lpesign;

using hamsterbyte.PoolMaster;

[RequireComponent(typeof(ObjectPool))]
public class GamePool : SingletonSimple<GamePool>
{
    ObjectPool _pool;
    override protected void Awake()
    {
        base.Awake();

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

    void Start()
    {
        _pool = PoolMaster.instance;
        // _pool.Preload("Symbols");
    }

    public Symbol SpawnSymbol(string objName) 
    {
		var pos = Vector3.one;
		var r = Quaternion.identity;

		GameObject go = _pool.SpawnReference( "Symbols", objName );

        if( go != null ) return go.GetComponent<Symbol>();
        return null;
    }

    public void DespawnSymbol( Symbol symbol )
    {
        _pool.Despawn( symbol.gameObject );
    }

    void OnPreloadPool(string poolName)
    {
        Debug.Log("@@@@@@@@@@@@@@@@@@ OnPreloadPool : " + poolName);
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
