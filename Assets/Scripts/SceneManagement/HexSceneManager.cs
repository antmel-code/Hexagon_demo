using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HexSceneManager : MonoBehaviour
{
    public void LoagGameScene()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void LoagEditorScene()
    {
        SceneManager.LoadScene(2);
    }

    public void LoagMainScene()
    {
        SceneManager.LoadScene(0);
    }

    public void Exit()
    {
        Application.Quit();
    }

}
