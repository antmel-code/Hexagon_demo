using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
{
    public static T Instance{ get => instance; private set => instance = value;}
    private static T instance;

    protected void Awake()
    {
        // For some reason, instances can be saved when a scene close
        /*
        if (Instance == null)
        {
            Instance = GetComponent<T>();
        }
        else
        {
            Destroy(this);
        }
        */
        Instance = GetComponent<T>();
    }

    private void OnDestroy()
    {
        Instance = default;
    }
}
