using UnityEngine;
using UnityEngine.UI;

public class SlotMachineUI : MonoBehaviour
{
    public Text TxtBalance;
    void Awake()
    {
        CanvasUtil.CanvasSetting(GetComponent<Canvas>());
    }

    void Start()
    {
        SetBalance(0);
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
