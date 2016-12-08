using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class HighDiamondsFreeSpinDirector : FreeSpinDirector
{
    public GameObject selector;
    public Button spin30;
    public Button spin15;
    public Button spin10;



    void Awake()
    {
        spin30.onClick.AddListener(() => OnClick(1));
        spin15.onClick.AddListener(() => OnClick(2));
        spin10.onClick.AddListener(() => OnClick(3));

        selector.gameObject.SetActive(false);
    }


    void OnClick(int index)
    {
        Debug.Log(" click: " + index);
        SelectedKind = index;
    }


    override public IEnumerator Select()
    {
        _openendTime = Time.time;

        selector.gameObject.SetActive(true);

        while (SelectedKind == null && ElapsedTime < _limitTime)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (SelectedKind == null) SelectedKind = Random.Range(0, 3) + 1;

        yield break;
    }
}
