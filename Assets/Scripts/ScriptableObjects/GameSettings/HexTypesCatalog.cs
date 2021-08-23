using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HexTypesCatalog", menuName = "HexTypesCatalog")]
public class HexTypesCatalog : ScriptableObject
{
    [SerializeField]
    HexType[] catalog;

    public static string Path
    {
        get => "GameSettings/HexTypesCatalog";
    }

    public static HexTypesCatalog Instance
    {
        get
        {
            HexTypesCatalog settings = Resources.Load<HexTypesCatalog>(Path);
            if (settings == null)
            {
                Debug.LogError("Could not find HexTypesCatalog.asset");
            }
            return settings;
        }
    }

    public Dictionary<string, HexType> HexTypesDictionary
    {
        get
        {
            Dictionary<string, HexType> dictionary = new Dictionary<string, HexType>();
            foreach (HexType type in catalog)
            {
                dictionary.Add(type.name, type);
            }
            return dictionary;
        }
    }
}
