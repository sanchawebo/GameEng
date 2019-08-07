using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadMainGame : MonoBehaviour
{
    void Start()
    {
        Invoke("MainGame", 3f);
    }

    void MainGame()
    {
        SceneManager.LoadScene(1);
    }
}
