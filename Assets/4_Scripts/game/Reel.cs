using UnityEngine;
using System.Collections;

public class Reel : MonoBehaviour
{
    // public int column{get;set;}

    int _column;
    public int Column
    {
        get { return _column; }
        set
        {
            _column = value;
            gameObject.name = "Reel" + _column;
        }
    }

	void Start()
	{
		// new <String>[ SR,EM,SG],
		// 		new <String>[ EM,WILD,EM],
		// 		new <String>[ SB,EM,SG]
	}

	public void SetSymbols()
	{

	}	
}
