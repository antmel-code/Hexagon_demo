using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Switch : MonoBehaviour
{
    [SerializeField]
    UnityEvent eventOn;
    [SerializeField]
    UnityEvent eventOff;
    bool isSwitched = false;

    public void DoSwitch()
    {
        if (isSwitched)
        {
            isSwitched = false;
            eventOff.Invoke();
        }
        else
        {
            isSwitched = true;
            eventOn.Invoke();
        }
    }
}
