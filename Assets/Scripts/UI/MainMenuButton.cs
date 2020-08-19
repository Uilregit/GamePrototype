﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    private void OnMouseDown()
    {
        //GameController.gameController.RestartGame();
        GameObject sacrificialLamb = new GameObject();
        DontDestroyOnLoad(sacrificialLamb);

        foreach (GameObject obj in sacrificialLamb.scene.GetRootGameObjects())
            Destroy(obj);

        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }
}
