using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using lpesign;

public class SlotModel : SingletonSimple<SlotModel>
{
    public event UnityAction<double> OnUpdateBalance;

    double _balance;
    public double Balance
    {
        get { return _balance; }
        private set
        {
            _balance = value;
            if (OnUpdateBalance != null) OnUpdateBalance(_balance);
        }
    }

    public float TotalBet { get; private set; }

    public void SetLoginData(ResDTO.Login dto)
    {
        Balance = dto.balance;
    }

    public void SetSpinData(ResDTO.Spin dto)
    {
        Balance = dto.balance;
    }
}
