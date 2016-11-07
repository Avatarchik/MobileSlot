/*

**************************************
************ POOL MASTER *************
**************************************
______________________________________

VERSION: 3.0
FILE:    POOLEDPARTICLE.CS
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

namespace hamsterbyte.PoolMaster
{
    public class PooledParticle : MonoBehaviour
    {
        private bool _loaded;
        public bool despawnOnInvisible = true;
        void Awake()
        {
            //Has the particle been loaded into memory yet? If not, we make sure it is disabled to refrain from playing
            //the particle until it has been loaded and called upon.
            if (!_loaded)
            {

                if (this.gameObject.activeSelf)
                    this.gameObject.SetActive(false);

                _loaded = true;
            }
        }
        void OnEnable()
        {
            //Start the PlayParticle coroutine
			if(_loaded)
            StartCoroutine("PlayParticle");
        }
        void OnDisable()
        {
            //Stop the PlayParticle coroutine
            StopAllCoroutines();
        }
        void OnBecameInvisible()
        {
			PooledObject pObj = this.GetComponent<PooledObject>();
			if(pObj != null)
				pObj.Despawn();
            StopAllCoroutines();
        }
        //This coroutine handles the playing of the particle. It is called on enable.
        public IEnumerator PlayParticle()
        {
            //Locate the particle system
            ParticleSystem ps = this.GetComponent<ParticleSystem>();
			if(!ps.isPlaying)
				ps.Play();
            if (ps.loop)
            {
                yield break;
            }
            else
            {
                //While this coroutine is playing and a particle system exists
                while (true && ps != null)
                {
                    //Wait for 1/4 second and check if the particle system is still playing, if it isn't, automatically disable the particle object.
                    yield return new WaitForSeconds(0.25f);
                    if (!ps.IsAlive(true))
                    {
						PooledObject pObj = this.GetComponent<PooledObject>();
						if(pObj != null)
							pObj.Despawn();
						
                        break;
                    }
                }
            }

        }
    }
}
