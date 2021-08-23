using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsPanel : MonoBehaviour
{
    private void Awake()
    {
        OptionExit();
        PlayerController.onOptionsExit += OptionExit;
        PlayerController.onOptionsEnter += OptionEnter;
    }

    private void OnDestroy()
    {
        PlayerController.onOptionsExit -= OptionExit;
        PlayerController.onOptionsEnter -= OptionEnter;
    }

    void OptionEnter()
    {
        gameObject.SetActive(true);
    }

    void OptionExit()
    {
        gameObject.SetActive(false);
    }
}
