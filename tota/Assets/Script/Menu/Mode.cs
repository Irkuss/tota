using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mode : MonoBehaviour
{
    public static Mode Instance;
    [HideInInspector] public bool online;
    private string _mode = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

    }

    public string ModeGame()
    {
        return _mode;
    }

    public void Solo()
    {
        _mode = "solo";
        if (!online) SceneManager.LoadScene(3);
    }   

    public void Cold()
    {
        _mode = "cold";
        if (!online) SceneManager.LoadScene(3);
    }

    public void Other()
    {
        _mode = "other";
        if (!online) SceneManager.LoadScene(3);
    }

    

}
