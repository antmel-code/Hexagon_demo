using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PathManager", menuName = "Path Manager")]
public class PathManager : ScriptableObject
{
    public static string Path
    {
        get => "GameSettings/PathManager";
    }

    public static PathManager Instance
    {
        get
        {
            PathManager manager = Resources.Load<PathManager>(Path);
            if (manager == null)
            {
                Debug.LogError("Could not find Path Manager.asset");
            }
            return manager;
        }
    }

    public string abilityIcons;
    public string resourceIcons;

}
