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
        _modes.SetActive(true);
        Mode.Instance.online = false;
        gameObject.SetActive(false);
    }

    public void MultGame()
    {
        PhotonNetwork.offlineMode = false;
        Mode.Instance.online = true;
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Solo()
    {
        Mode.Instance.Solo();
    }

    public void Cold()
    {
        Mode.Instance.Cold();
    }

    public void Other()
    {
        Mode.Instance.Other();
    }

    public void BackMenu()
    {
        Debug.Log("back");
        _modes.SetActive(false);
        gameObject.SetActive(true);
    }


}
