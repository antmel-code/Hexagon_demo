using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingType", menuName = "BuildingType")]
public class BuildingType : ScriptableObject
{
    [SerializeField]
    GameObject building;

    [SerializeField]
    bool isOrientToWater = false;

    public GameObject Building
    {
        get => building;
    }

    public bool IsOrientToWather
    {
        get => isOrientToWater;
    }
}
