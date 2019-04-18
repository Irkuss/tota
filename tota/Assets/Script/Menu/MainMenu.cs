using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _modes = null;

    public void SoloGame()
    {
        PhotonNetwork.offlineMode = true;
        //SceneManager.LoadScene(3);
        _modes.SetActive(true);
        Mode.Instance.online = false;
        gameObject.SetActive(false);
    }

    public void MultGame()
    {
        PhotonNetwork.offlineMode = false;
        //SceneManager.LoadScene(2); // When solo will be done get the index to 2
        _modes.SetActive(true);
        Mode.Instance.online = true;
        gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    
}
