using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SlotUI : MonoBehaviour
{
    public Text TxtBalance;
    void Awake()
    {
        CanvasUtil.CanvasSetting(GetComponent<Canvas>());
    }

	void Start()
	{
		SlotModel.Instance.OnUpdateBalance += ( balance ) => DisplayBalance( balance );
	}

    public void Idle()
    {
		DisplayBalance( SlotModel.Instance.Balance );
    }

	void DisplayBalance( double balance )
	{
		TxtBalance.text = balance.ToString("#,##0");
	}
}
