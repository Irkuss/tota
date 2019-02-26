using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public void SoloGame()
    {
        PhotonNetwork.offlineMode = true;
        SceneManager.LoadScene(3);
    }

    public void MultGame()
    {
        SceneManager.LoadScene(2); // When solo will be done get the index to 2
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    
}
