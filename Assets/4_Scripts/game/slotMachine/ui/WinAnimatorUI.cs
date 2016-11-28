using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class WinAnimatorUI : MonoBehaviour
{
    public CanvasGroup content;
    public Text txtWin;

    double _win;
    RectTransform _contentRtf;

    void Awake()
    {
        Hide();
    }

    public void Init()
    {
        content.interactable = false;
        content.blocksRaycasts = false;
        _contentRtf = content.gameObject.GetComponent<RectTransform>();

        Reset();
    }

    void Reset()
    {
        _win = 0;
        _contentRtf.localScale = Vector3.one;

        txtWin.text = "";
        content.alpha = 0f;
    }

    void Hide()
    {
        content.gameObject.SetActive(false);
    }

    void Show()
    {
        content.gameObject.SetActive(true);
    }

    public void AddWin(WinBalanceInfo info)
    {
        Show();

        content.DOFade(1f, 0.2f).Play();
        SetWin(0);
        SetWin(info.balance, info.duration);
    }

    void SetWin(double win, float duration = 0f)
    {
        if (win == _win || duration == 0f) UpdateWin(win);
        else DOTween.To(() => _win, x => UpdateWin(x), win, duration).OnComplete(WinComplete).Play();
    }

    void UpdateWin(double win)
    {
        _win = win;
        txtWin.text = _win.ToBalance();
    }

    void WinComplete()
    {
        var sequence = DOTween.Sequence();
        sequence.PrependInterval(0.3f);
        sequence.Append(_contentRtf.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.InCubic));
        sequence.Join(content.DOFade(0f, 0.2f).SetEase(Ease.InCubic));
        sequence.AppendCallback(Reset);
        sequence.Play();
    }
}
