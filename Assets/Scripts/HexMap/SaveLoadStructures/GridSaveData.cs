using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GridSaveData
{
    public int cellCountX, cellCountZ;
    public CellSaveData[] cellData;
}
