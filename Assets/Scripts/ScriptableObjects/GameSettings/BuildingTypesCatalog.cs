using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingTypesCatalog", menuName = "Building Types Catalog")]
public class BuildingTypesCatalog : ScriptableObject
{
    [SerializeField]
    BuildingType[] catalog;

    public static string Path
    {
        get => "GameSettings/BuildingTypesCatalog";
    }

    public static BuildingTypesCatalog Instance
    {
        get
        {
            BuildingTypesCatalog settings = Resources.Load<BuildingTypesCatalog>(Path);
            if (settings == null)
            {
                Debug.LogError("Could not find BuildingTypesCatalog.asset");
            }
            return settings;
        }
    }

    public Dictionary<string, BuildingType> BuildingTypesDictionary
    {
        get
        {
            Dictionary<string, BuildingType> dictionary = new Dictionary<string, BuildingType>();
            foreach (BuildingType type in catalog)
            {
                dictionary.Add(type.name, type);
            }
            return dictionary;
        }
    }
}
