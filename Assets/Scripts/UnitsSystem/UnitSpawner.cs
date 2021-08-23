using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField]
    Unit unitPrefab;

    bool isEarlyCreated = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!HexMapMaster.Instance.IsMapCreated)
        {
            HexMapMaster.onMapCreated += SpawnUnits;
            isEarlyCreated = true;
        }
        else
            SpawnUnits();
    }

    void SpawnUnits()
    {
        HexCoordinates hex = AnyNoWaterHex;
        HexMapMaster.Instance.SpawnUnit(hex, "Colonialists");

        HexMapMaster.Instance.LookAtCell(hex);

        if (isEarlyCreated)
            HexMapMaster.onMapCreated -= SpawnUnits;
    }

    HexCoordinates AnyNoWaterHex
    {
        get
        {
            HexCoordinates hex = new HexCoordinates(Random.Range(0, 100), Random.Range(0, 24)); // Bad cod (magic number)
            int i = 0;
            while ((HexMapMaster.Instance.GetCellOnMap(hex) == null || HexMapMaster.Instance.GetCellOnMap(hex).IsWater) && i < 100)
            {
                hex = new HexCoordinates(Random.Range(0, 100), Random.Range(0, 24)); // Bad cod (magic number)
                i++;
            }
            return hex;
        }
        
    }
}
