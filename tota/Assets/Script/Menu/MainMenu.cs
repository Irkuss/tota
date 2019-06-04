using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject _modes = null;
    [SerializeField] private GameObject _loadButton = null;
    [SerializeField] private GameObject _launchButton = null;
    [SerializeField] private GameObject _loadText = null;

    private void Start()
    {
        AudioManager.instance.Play("Menu");
    }

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

    public void Zombie()
    {
        Mode.Instance.Zombie();
    }

    public void BackMenu()
    {
        _modes.SetActive(false);
        gameObject.SetActive(true);
        _loadButton.SetActive(true);
        _launchButton.SetActive(false);
        _loadText.SetActive(false);
    }
}
