using UnityEngine;
using UnityEngine.UI;

public class SlotMachineUI : MonoBehaviour
{
    public Text TxtBalance;

    [Header("Buttons")]
    public Button btnPaytable;
    public Button btnBetDecrease;
    public Button btnBetIncrease;
    public Button btnFast;
    public Button btnAuto;
    public Button btnSpin;

    SlotMachine _slot;

    InfoViewUI _info;

    void Awake()
    {
        CanvasUtil.CanvasSetting(GetComponent<Canvas>());

        _info = GetComponentInChildren<InfoViewUI>();
        if( _info == null ) Debug.LogError("can't find InfoViewUI");
    }

    void Start()
    {
        SetBalance(0);
    }

    public void Initialize(SlotMachine slot)
    {
        _slot = slot;

        InitInfo();
        InitButtons();
    }

    void InitInfo()
    {
        _info.Init( _slot );
    }

    void InitButtons()
    {
        btnPaytable.onClick.AddListener(() =>
        {
            Debug.Log("show Paytable");
        });

        btnBetDecrease.onClick.AddListener(() =>
        {
            Debug.Log("---");
        });

        btnBetIncrease.onClick.AddListener(() =>
        {
            Debug.Log("+++");
        });

        btnFast.onClick.AddListener(() =>
        {
            Debug.Log("fast");
        });

        btnAuto.onClick.AddListener(() =>
        {
            Debug.Log("auto");
        });

        btnSpin.onClick.AddListener(() =>
        {
            Debug.Log("spin");
            _slot.TrySpin();
        });
    }

    public void Idle()
    {
        UpdateBalance();
    }

    public void UpdateBalance()
    {
        SetBalance(SlotModel.Instance.Owner.Balance);
    }

    void SetBalance(double balance)
    {
        TxtBalance.text = balance.ToString("#,##0");
    }
}
