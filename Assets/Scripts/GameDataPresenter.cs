using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataPresenter : Singleton<GameDataPresenter>
{
    Dictionary<string, UnitArchitype> unitArchitypesDictionary;

    Dictionary<string, HexType> hexTypesDictionary;

    Dictionary<string, BuildingType> buildingTypesDictionary;

    Dictionary<HexWaterType, Color> waterColorsDictionary;

    Dictionary<PlayerIdentifier, Color> playerColors;

    PathManager pathManager;

    HexMetrics hexMetrics;

    public PathManager PathManager
    {
        get => pathManager;
    }

    public HexMetrics HexMetrics
    {
        get => hexMetrics;
    }

    private void Awake()
    {
        base.Awake();
        InitData();
    }

    void InitData()
    {
        unitArchitypesDictionary = UnitArchitypesCatalog.Instance.UnitArchitypeDictionary;
        waterColorsDictionary = HexWaterColors.Instance.WaterColorsDictionary;
        hexTypesDictionary = HexTypesCatalog.Instance.HexTypesDictionary;
        buildingTypesDictionary = BuildingTypesCatalog.Instance.BuildingTypesDictionary;
        playerColors = PlayerColors.Instance.PlayerColorsDictionary;
        pathManager = PathManager.Instance;
        hexMetrics = HexMetrics.Instance;
    }

    public UnitArchitype GetUnitArchitypeByName(string name)
    {
        if (unitArchitypesDictionary.ContainsKey(name))
        {
            return unitArchitypesDictionary[name];
        }
        else
            return null;
    }

    public HexType GetHexTypeByName(string name)
    {
        if (hexTypesDictionary.ContainsKey(name))
            return hexTypesDictionary[name];
        else
            return null;
    }

    public BuildingType GetBuildingTypeByName(string name)
    {
        if (buildingTypesDictionary.ContainsKey(name))
        {
            return buildingTypesDictionary[name];
        }
        else
        {
            return null;
        }
    }

    public Color GetWatherColorByType(HexWaterType type)
    {
        if (waterColorsDictionary.ContainsKey(type))
            return waterColorsDictionary[type];
        else
            return Color.white;
    }

    public Color GetPlayerColor(PlayerIdentifier player)
    {
        if (playerColors.ContainsKey(player))
            return playerColors[player];
        else
            return Color.white;
    }

    public Color GetWatherColorByName(string name)
    {
        return GetWatherColorByType(GetHexTypeByName(name).WaterType);
    }

    public Sprite GetSpriteByResourceType(ResourceType type)
    {
        return Resources.Load<Sprite>(pathManager.resourceIcons + "/" + type.ToString());
    }

    public Sprite GetSpriteByAbilityType(AbilityType type)
    {
        return Resources.Load<Sprite>(pathManager.abilityIcons + "/" + type.ToString());
    }
}
