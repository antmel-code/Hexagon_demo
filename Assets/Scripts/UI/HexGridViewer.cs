using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class HexGridViewer : MonoBehaviour
{
    Material material;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetOpacity(float value)
    {
        material.SetFloat("_Opacity", value);
    }

    public void SetWaves(float value)
    {
        material.SetFloat("_Waves", value);
    }

    public void SetHighlight(float value)
    {
        material.SetFloat("_Highlight", value);
    }
}
