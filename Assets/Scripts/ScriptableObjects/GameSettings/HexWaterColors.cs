using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HexWaterColorSettings", menuName = "HexWaterColorSettings")]
public class HexWaterColors : ScriptableObject
{

    public static string Path
    {
        get => "GameSettings/HexWaterColorSettings";
    }

    public static HexWaterColors Instance
    {
        get
        {
            HexWaterColors settings = Resources.Load<HexWaterColors>(Path);
            if (settings == null)
            {
                Debug.LogError("Could not find HexWaterColorSettings.asset");
            }
            return settings;
        }
    }

    [System.Serializable]
    public struct WaterTypeColor
    {
        public HexWaterType waterType;
        public Color color;
    }

    [SerializeField]
    WaterTypeColor[] waterColors;

    public Dictionary<HexWaterType, Color> WaterColorsDictionary
    {
        get
        {
            Dictionary<HexWaterType, Color> dictionary = new Dictionary<HexWaterType, Color>();
            foreach (WaterTypeColor pair in waterColors)
            {
                dictionary.Add(pair.waterType, pair.color);
            }
            return dictionary;
        }
    }
}
