using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    bool isThereBuilding = false;

    int cityIndex = 0;

    public HexCoordinates coordinates;

    public HexGridChunk chunk;

    [SerializeField]
    bool[] roads;

    int elevation;

    PlayerIdentifier owner = PlayerIdentifier.None;

    public RectTransform coordRect;
    public RectTransform resourceRect;

    string nameOfHexType = "SeaHex";

    string nameOfBuildingType = "Castle";

    public int CityIndex
    {
        get => cityIndex;
        set => cityIndex = value;
    }

    public PlayerIdentifier Owner
    {
        get => owner;
        set
        {
            if (owner != value)
            {
                owner = value;
                Refresh();
            }
        }
    }

    public bool IsThereBuilding
    {
        get => isThereBuilding;
    }

    public string NameOfHexType
    {
        get => nameOfHexType;
        set
        {
            nameOfHexType = value;
            Refresh();
        }
    }

    public ResourceType[] Resources
    {
        get => HexTypeInfo.Resources;
    }

    public string NameOfBuildingType
    {
        get => isThereBuilding ? nameOfBuildingType : "None";
    }

    HexType HexTypeInfo
    {
        get => GameDataPresenter.Instance.GetHexTypeByName(nameOfHexType);
    }

    public GameObject[] Features
    {
        get => HexTypeInfo.Features;
    }

    BuildingType BuildingInfo
    {
        get => GameDataPresenter.Instance.GetBuildingTypeByName(nameOfBuildingType);
    }

    public GameObject Building
    {
        get => BuildingInfo.Building;
    }

    public HexTerrainType TerrainType
    {
        get => HexTypeInfo.TerrainType;
    }

    public CellSaveData SaveData
    {
        get
        {
            CellSaveData data = new CellSaveData();
            data.hexCellType = NameOfHexType;
            data.buildingName = NameOfBuildingType;
            data.isThereBuilding = isThereBuilding;
            data.ownerIndex = 0;
            data.roadsTroughEdges = roads;
            data.elevation = elevation;
            return data;
        }
    }

    public int Elevation
    {
        get => elevation;
        set
        {
            elevation = value;
            UpdateElevation();
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
                {
                    SetRoad((HexDirection)i, false);
                }
            }
            Refresh();
        }
    }

    public bool IsWater
    {
        get => HexTypeInfo.IsWather;
    }

    public bool HasRoad
    {
        get
        {
            for (int i = 0; i < roads.Length; i++)
            {
                if (roads[i])
                {
                    return true;
                }
            }
            return false;
        }
    }

    void UpdateElevation()
    {
        Vector3 position = transform.position;
        position.y = elevation * GameDataPresenter.Instance.HexMetrics.ElevationStep;
        transform.position = position;
        Vector3 uiPosition = coordRect.localPosition;
        uiPosition.z = elevation * -GameDataPresenter.Instance.HexMetrics.ElevationStep;
        coordRect.localPosition = resourceRect.localPosition = uiPosition;
    }

    public void LoadCell(CellSaveData data)
    {
        nameOfHexType = data.hexCellType;
        roads = data.roadsTroughEdges;
        isThereBuilding = data.isThereBuilding;
        nameOfBuildingType = data.buildingName;
        elevation = data.elevation;
        UpdateElevation();
    }

    public bool HasRoadThroughEdge(HexDirection direction)
    {
        return roads[(int)direction];
    }

    public void RemoveRoads()
    {
        for (int i = 0; i < roads.Length; i++)
        {
            if (roads[i])
            {
                SetRoad((HexDirection)i, false);
            }
        }
    }

    public void AddRoad(HexDirection direction)
    {
        if (!roads[(int)direction] && GetElevationDifference(direction) <= 1)
        {
            SetRoad(direction, true);
        }
    }

    public void Build(string nameOfBuilding, PlayerIdentifier owner = PlayerIdentifier.Player0)
    {
        nameOfBuildingType = nameOfBuilding;
        this.owner = owner;
        isThereBuilding = true;
        Refresh();
    }

    public void RemoveBuilding()
    {
        isThereBuilding = false;
        Refresh();
    }

    int GetElevationDifference(HexDirection direction)
    {
        return Mathf.Abs(elevation - GetNeighbor(direction).elevation);
    }

    void SetRoad(HexDirection direction, bool state)
    {
        roads[(int)direction] = state;
        GetNeighbor(direction).roads[(int)direction.Opposite()] = state;
        GetNeighbor(direction).Refresh();
        Refresh();
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
       return HexMapMaster.Instance.GetCellOnMap(coordinates.GetNeighbor(direction));
    }

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return GameDataPresenter.Instance.HexMetrics.GetEdgeType(elevation, GetNeighbor(direction).elevation);
    }

    void Refresh()
    {
        if (!chunk)
        {
            return;
        }
        chunk.Refresh();
        for (int i = 0; i < 6; i++)
        {
            HexCell neighbor = GetNeighbor((HexDirection)i);
            if (neighbor != null && neighbor.chunk != chunk)
            {
                neighbor.chunk.Refresh();
            }
        }
    }
}
