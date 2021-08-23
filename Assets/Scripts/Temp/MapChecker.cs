using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

[RequireComponent(typeof(Button))]
public class MapChecker : MonoBehaviour
{
    [SerializeField]
    GameObject[] notes;
    // Start is called before the first frame update
    void Start()
    {
        string filePath = Application.persistentDataPath + "/map.map";

        if (File.Exists(filePath))
        {
            GetComponent<Button>().interactable = true;
            if (notes != null && notes.Length > 0)
            {
                foreach(GameObject obj in notes)
                {
                    obj.SetActive(false);
                }
            }
        }
        else
        {
            GetComponent<Button>().interactable = false;
            if (notes != null && notes.Length > 0)
            {
                foreach (GameObject obj in notes)
                {
                    obj.SetActive(true);
                }
            }
        }
    }
}
