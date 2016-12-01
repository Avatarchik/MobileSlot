using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class PaytableUI : AbstractSlotMachineUIModule
{
    public Image page;
    public Sprite[] pageList;

    [Header("Buttons")]
    public Button btnBack;
    public Button btnPrev;
    public Button btnNext;

    int _curerntPageIndex;
    int _lastPageIndex;

    CanvasGroup _group;

    void Awake()
    {
        _lastPageIndex = pageList.Length - 1;

        if (btnBack != null) btnBack.onClick.AddListener(PrevPage);
        if (btnNext != null) btnNext.onClick.AddListener(NextPage);
        btnBack.onClick.AddListener(Close);

        _group = GetComponent<CanvasGroup>();

        SetPage(0);
        Close();
    }

    public void Open()
    {
        _group.alpha = 1;
        _group.interactable = true;
        _group.blocksRaycasts = true;

        SetPage(0);
    }

    public void Close()
    {
        _group.alpha = 0;
        _group.interactable = false;
        _group.blocksRaycasts = false;
    }

    void SetPage(int page)
    {
        _curerntPageIndex = Mathf.Clamp(page, 0, _lastPageIndex);
        UpdatePage();
        UpdateButtonState();
    }

    void PrevPage()
    {
        SetPage(_curerntPageIndex - 1);
    }

    void NextPage()
    {
        SetPage(_curerntPageIndex + 1);
    }

    void UpdatePage()
    {
        Sprite sprite = pageList[_curerntPageIndex];
        page.sprite = sprite;
    }

    void UpdateButtonState()
    {
        if (_curerntPageIndex <= 0)
        {
            if (btnPrev != null) btnPrev.interactable = false;
            if (btnNext != null) btnNext.interactable = true;
        }
        else if (_curerntPageIndex >= _lastPageIndex)
        {
            if (btnPrev != null) btnPrev.interactable = true;
            if (btnNext != null) btnNext.interactable = false;
        }
        else
        {
            if (btnPrev != null) btnPrev.interactable = true;
            if (btnNext != null) btnNext.interactable = true;
        }
    }
}
