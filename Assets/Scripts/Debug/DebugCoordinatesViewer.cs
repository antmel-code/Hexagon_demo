using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HexGrid))]
public class DebugCoordinatesViewer : MonoBehaviour
{
    [SerializeField]
    Material terrainMaterial;

    [SerializeField]
    Material waterMaterial;

    [SerializeField]
    bool show = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (show)
        {
            ShowGrid(true);
            GetComponent<HexGrid>().ShowCoords();
        }
        else
        {
            ShowGrid(false);
            GetComponent<HexGrid>().HideCanvas();
        }
        enabled = false;
    }

    public void ShowGrid(bool visible)
    {
        if (visible)
        {
            terrainMaterial.EnableKeyword("GRID_ON");
            waterMaterial.EnableKeyword("GRID_ON");
        }
        else
        {
            terrainMaterial.DisableKeyword("GRID_ON");
            waterMaterial.DisableKeyword("GRID_ON");
        }
    }
}
