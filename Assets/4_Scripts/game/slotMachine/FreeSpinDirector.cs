using UnityEngine;
using System.Collections;

namespace Game
{
    public abstract class FreeSpinDirector : MonoBehaviour
    {
        [SerializeField]
        protected float _limitTime = 15f;

        public GameObject selector;

        protected float _openendTime;

        public int? SelectedKind { get; protected set; }

        virtual public IEnumerator FreeSpinReady()
        {
            yield break;
        }

        virtual public IEnumerator Trigger()
        {
            yield break;
        }

        virtual public IEnumerator Retrigger()
        {
            yield break;
        }

        virtual public IEnumerator Select()
        {
            yield break;
        }

        virtual public IEnumerator Summary()
        {
            yield break;
        }

        virtual public void Close()
        {

        }

        protected float ElapsedTime
        {
            get { return Time.time - _openendTime; }
        }
    }
}
