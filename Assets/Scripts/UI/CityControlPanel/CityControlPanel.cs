using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityControlPanel : MonoBehaviour
{
    private void Awake()
    {
        Deactivate();
        PlayerController.onCitiesManagingModEnter += Activate;
        PlayerController.onCitiesManagingModExit += Deactivate;
    }

    private void OnDestroy()
    {
        PlayerController.onCitiesManagingModEnter -= Activate;
        PlayerController.onCitiesManagingModExit -= Deactivate;
    }

    void Activate()
    {
        gameObject.SetActive(true);
    }

    void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
