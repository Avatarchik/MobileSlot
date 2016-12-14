using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Game.HighDiamonds
{
    public class HighDiamondsFreeSpinDirector : FreeSpinDirector
    {
        public Button spin30;
        public Button spin15;
        public Button spin10;

        CanvasGroup _selectorGroup;

        void Awake()
        {
            spin30.onClick.AddListener(() => OnClick(1));
            spin15.onClick.AddListener(() => OnClick(2));
            spin10.onClick.AddListener(() => OnClick(3));

            _selectorGroup = selector.gameObject.GetComponent<CanvasGroup>();

            _selectorGroup.alpha = 0;
            _selectorGroup.interactable = false;
            _selectorGroup.blocksRaycasts = false;
        }


        void OnClick(int index)
        {
            Debug.Log(" click: " + index);
            SelectedKind = index;
        }


        override public IEnumerator Select()
        {
            yield return new WaitForSeconds(0.2f);

            yield return StartCoroutine(_selectorGroup.FadeTo(0f, 1f, 0.3f));

            _openendTime = Time.time;
            _selectorGroup.interactable = true;
            _selectorGroup.blocksRaycasts = true;

            selector.gameObject.SetActive(true);

            while (SelectedKind == null && ElapsedTime < _limitTime)
            {
                yield return new WaitForSeconds(0.1f);
            }

            _selectorGroup.interactable = false;
            _selectorGroup.blocksRaycasts = false;

            if (SelectedKind == null) SelectedKind = Random.Range(0, 3) + 1;
            yield break;
        }

        override public void Close()
        {
            StartCoroutine(_selectorGroup.FadeTo(1f, 0f, 0.2f));
        }
    }
}
