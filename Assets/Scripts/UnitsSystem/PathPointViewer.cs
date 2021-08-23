using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathPointViewer : MonoBehaviour
{
    public enum PathPointType { Normal, Warning, Danger }

    [SerializeField]
    Text text;

    [SerializeField]
    Graphic typeIndicator;

    [SerializeField]
    Color warningColor = Color.yellow;

    [SerializeField]
    Color dangerColor = Color.red;

    int cost = 0;

    PathPointType type;

    public int Cost
    {
        get => cost;
        set
        {
            cost = value;
            text.text = value.ToString();
        }
    }

    public PathPointType Type
    {
        get => type;
        set
        {
            type = value;
            if (type == PathPointType.Danger)
            {
                typeIndicator.color = dangerColor;
            }
            else if (type == PathPointType.Warning)
            {
                typeIndicator.color = warningColor;
            }
            else
            {
                typeIndicator.color = Color.white;
            }
        }
    }
}
