using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[RequireComponent(typeof(HexGrid))]
public class HexGridSaver : MonoBehaviour
{
    [SerializeField]
    bool loadOnStart = false;

    HexGrid grid;

    private void Awake()
    {
        grid = GetComponent<HexGrid>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (loadOnStart)
        {
            Load();
        }
    }

    public bool Save()
    {
        string savePath = Application.persistentDataPath + "/map.map";

        GridSaveData save = grid.SaveData;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(savePath);

        bf.Serialize(file, save);
        file.Close();

        Debug.Log("Map has been saved in " + savePath);

        return true;
    }

    public bool Load()
    {
        string filePath = Application.persistentDataPath + "/map.map";

        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);

            GridSaveData save = (GridSaveData)bf.Deserialize(file);
            file.Close();

            grid.LoadMap(save);

            Debug.Log("Map has been loaded from " + filePath);
        }
        else
        {
            Debug.LogWarning("Filed to load map from " + filePath);

            return false;
        }

        return true;
    } 
}
