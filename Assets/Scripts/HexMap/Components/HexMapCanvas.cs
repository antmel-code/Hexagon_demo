using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexMapCanvas : MonoBehaviour
{
    [SerializeField]
    RectTransform coords;

    [SerializeField]
    RectTransform resources;

    public RectTransform Coords
    {
        get => coords;
    }

    public RectTransform Resources
    {
        get => resources;
    }

    public void ShowCoords()
    {
        coords.gameObject.SetActive(true);
        resources.gameObject.SetActive(false);
    }

    public void ShowResources()
    {
        coords.gameObject.SetActive(false);
        resources.gameObject.SetActive(true);
    }

    public void HideCanvas()
    {
        coords.gameObject.SetActive(false);
        resources.gameObject.SetActive(false);
    }

    public void RefreshResources()
    {
        CellResourceViewer[] viewers = GetComponentsInChildren<CellResourceViewer>();
        foreach (CellResourceViewer viewer in viewers)
        {
            viewer.Refresh();
        }
    }
}
