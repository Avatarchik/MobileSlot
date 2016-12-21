using UnityEngine;
using System.Collections;

public class DetectOnDestroy : MonoBehaviour
{
    void OnDestroy()
    {
        Debug.Log("OnDestroy: " + gameObject.name + "(" + nick + ")");
    }

    public string nick;
}
