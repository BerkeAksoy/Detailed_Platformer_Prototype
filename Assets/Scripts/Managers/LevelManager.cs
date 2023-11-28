using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;

    public static LevelManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("Level Manager is null.");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void loadMainMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void loadOptionsMenu()
    {

    }

    public void loadTown()
    {
        SceneManager.LoadScene(3);
    }

    public void loadLevelSelectionScene()
    {

    }

    public void restartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void loadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }



}
