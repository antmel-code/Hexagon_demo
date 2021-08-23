using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HexType", menuName = "HexType")]
public class HexType : ScriptableObject
{
    [SerializeField]
    HexTerrainType terrainType;

    [SerializeField]
    GameObject[] features;

    [SerializeField]
    ResourceType[] resources;

    [SerializeField]
    HexWaterType waterType;

    public HexTerrainType TerrainType
    {
        get => terrainType;
    }

    public GameObject[] Features
    {
        get => (GameObject[])features.Clone();
    }

    public ResourceType[] Resources
    {
        get => resources != null ? (ResourceType[])resources.Clone() : new ResourceType[0];
    }

    public HexWaterType WaterType
    {
        get => waterType;
    }

    public bool IsWather
    {
        get => waterType != HexWaterType.None;
    }
}
