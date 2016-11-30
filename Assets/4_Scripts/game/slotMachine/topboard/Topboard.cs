﻿using UnityEngine;
using System.Collections;

using DG.Tweening;

public class Topboard : MonoBehaviour
{
	//todo
	//보너스 스핀, 빅윈 메가윈 잭팟 등등의 연출은 트윈이 사용되었는데 애니메이션 클립 만들어서 재생시키자

    public SpriteRenderer bonusSpin;
    public SpriteRenderer bigwin;
    public SpriteRenderer megawin;
    public SpriteRenderer jackpotwin;

    WinTableModule _winTableModule;

    void Awake()
    {
        _winTableModule = GetComponentInChildren<WinTableModule>();

        if (bonusSpin != null) bonusSpin.enabled = false;
        if (bigwin != null) bigwin.enabled = false;
        if (megawin != null) megawin.enabled = false;
        if (jackpotwin != null) jackpotwin.enabled = false;
    }

    public void Spin()
    {
        if (_winTableModule != null) _winTableModule.Clear();
    }

    public void PlayAllWin(WinItemList info)
    {
        if (_winTableModule != null) _winTableModule.PlayAllWin(info);
    }

    public void PlayEachWin(WinItemList.Item item)
    {
        if (_winTableModule != null) _winTableModule.PlayEachWin(item);
    }

    public void BonusSpin()
    {
        if (bonusSpin == null) return;

		HideWinTable();

		bonusSpin.enabled = true;
		bonusSpin.transform.localScale = Vector3.zero;
		bonusSpin.color = new Color( bonusSpin.color.r, bonusSpin.color.g, bonusSpin.color.b,0f);

        Sequence spinAnim = DOTween.Sequence();

        spinAnim.Append(bonusSpin.transform.DOScale(1f, 0.3f)
                                     			.SetEase(Ease.OutCubic));
		spinAnim.Join(bonusSpin.DOFade(1f,0.3f));

        spinAnim.AppendInterval(0.7f);

        spinAnim.Append(bonusSpin.transform.DOScale(1.5f, 0.3f)
                                     			.SetEase(Ease.OutCubic).OnStart( ShowWinTable ));
		spinAnim.Join(bonusSpin.DOFade(0f,0.3f));

        spinAnim.Play();
    }

	void HideWinTable()
	{
		if (_winTableModule != null) _winTableModule.gameObject.SetActive(false);
	}

	void ShowWinTable()
	{
		if (_winTableModule != null) _winTableModule.gameObject.SetActive(true);
	}
}