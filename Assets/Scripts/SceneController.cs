using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{

    public void OnCubeRandomizationButton()
    {
        SceneManager.LoadScene("RandomCubesScene");
    }

    public void OnGameOfLifeButton()
    {
        SceneManager.LoadScene("GameOfLife");
    }

    public void OnBoidsButton()
    {
        SceneManager.LoadScene("BoidsScene");
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("SceneLoader");
    }


}
