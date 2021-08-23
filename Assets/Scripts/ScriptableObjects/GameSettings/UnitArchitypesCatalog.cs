using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitArchitypesCatalog", menuName = "Unit Architypes Catalog")]
public class UnitArchitypesCatalog : ScriptableObject
{
    [SerializeField]
    UnitArchitype[] catalog;

    public static string Path
    {
        get => "GameSettings/UnitArchitypesCatalog";
    }

    public static UnitArchitypesCatalog Instance
    {
        get
        {
            UnitArchitypesCatalog settings = Resources.Load<UnitArchitypesCatalog>(Path);
            if (settings == null)
            {
                Debug.LogError("Could not find UnitArchitypesCatalog.asset");
            }
            return settings;
        }
    }

    public Dictionary<string, UnitArchitype> UnitArchitypeDictionary
    {
        get
        {
            Dictionary<string, UnitArchitype> dictionary = new Dictionary<string, UnitArchitype>();
            foreach (UnitArchitype type in catalog)
            {
                dictionary.Add(type.name, type);
            }
            return dictionary;
        }
    }
}
