using UnityEngine;
using System.Collections;

namespace Game
{
    public class DebugHelper : MonoBehaviour
    {
        SlotMachine _machine;
        void Start()
        {
            _machine = FindObjectOfType<SlotMachine>();
        }

        void Update()
        {
            TestSpin();
        }

        void TestSpin()
        {
            if (_machine == null)
            {
                return;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                TestSpin("spin_case_a");
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                TestSpin("spin_case_b");
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                TestSpin("spin_case_c");
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                TestSpin("spin_case_d");
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                TestSpin("spin_case_e");
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                TestSpin("spin_case_f");
            }
            else if (Input.GetKeyDown(KeyCode.J))
            {
                TestSpin("spin_case_j");
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                TestSpin("spin_case_s");
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                TestSpin("spin_case_r");
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                TestSpin("spin_case_w");
            }
        }

        void TestSpin(string command)
        {
            _machine.TrySpin(new SendData()
            {
                cmd = command,
                data = new ReqDTO.Spin() { lineBet = _machine.Config.Betting.LineBet }
            }); ;
        }

        void GetTestSpinSendData(string commnad)
        {

        }
    }
}
