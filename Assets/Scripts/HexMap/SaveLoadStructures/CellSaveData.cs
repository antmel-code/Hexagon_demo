using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CellSaveData
{
    public string hexCellType;
    public int elevation;
    public bool isThereBuilding;
    public string buildingName;
    public int ownerIndex;
    public bool[] roadsTroughEdges;
}
