using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace Game
{
    public class WinAnimatorUI : AbstractSlotMachineUIModule
    {
        public CanvasGroup content;
        public Text txtWin;

        double _win;
        RectTransform _contentRtf;
        Tweener _tweenWin;

        void Awake()
        {
            Hide();
        }

        override public void Init(SlotMachineUI slotUI)
        {
            base.Init(slotUI);
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

        public void SkipTakeCoin()
        {
            if (_tweenWin != null) _tweenWin.Complete(true);
        }

        public void AddWin(WinBalanceInfo info)
        {
            Show();

            content.DOFade(1f, 0.2f).Play();
            SetWin(0);
            SetWin(info.win, info.duration);
        }

        void SetWin(double win, float duration = 0f)
        {
            if (win == _win || duration == 0f) UpdateWin(win);
            else _tweenWin = DOTween.To(() => _win, x => UpdateWin(x), win, duration).OnComplete(WinComplete).Play();
        }

        void UpdateWin(double win)
        {
            _win = win;
            txtWin.text = _win.ToBalance();
        }

        void WinComplete()
        {
            var sequence = DOTween.Sequence();
            sequence.PrependInterval(0.5f);
            sequence.Append(_contentRtf.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.InCubic));
            sequence.Join(content.DOFade(0f, 0.2f).SetEase(Ease.InCubic));
            sequence.AppendCallback(Reset);
            sequence.Play();

            _tweenWin = null;
        }
    }
}
