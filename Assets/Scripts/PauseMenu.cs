using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public static bool GameIsPaused = false;

    public GameObject pauseMenu;

    public AudioSource music;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            } else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        music.UnPause();
        pauseMenu.SetActive(false);
        GameIsPaused = false;
        Time.timeScale = 1f;
    }

    void Pause()
    {
        music.Pause();
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        GameIsPaused = true;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

}
