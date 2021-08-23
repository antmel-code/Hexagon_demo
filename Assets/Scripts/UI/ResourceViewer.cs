using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceViewer : MonoBehaviour
{
    [SerializeField]
    Image sprite;

    public Image ResourceSprite
    {
        get => sprite;
    }
    
}
