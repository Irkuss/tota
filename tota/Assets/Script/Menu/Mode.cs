using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Mode : MonoBehaviour
{
    public static Mode Instance;
    private string _mode = "";

    public bool ShouldZombieSpawn => _mode == "zombie";
    public bool ShouldTemperatureBeModified => _mode == "cold";
    public bool ShouldAiSpawn => _mode != "solo";
    
    [HideInInspector] public bool online;   

    [HideInInspector] public bool zqsd = false;
    [HideInInspector] public string interfaCe = "e";
    [HideInInspector] public string channel = "c";
    [HideInInspector] public string build = "b";

    [HideInInspector] public bool load = false;
    [HideInInspector] public int firstTime = 0;
    [HideInInspector] public bool isSkip = false;
    [SerializeField] private GameObject _loadButton = null;
    [SerializeField] private GameObject _launchButton = null;
    [SerializeField] private GameObject _loadText = null;

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

    public void Zombie()
    {
        _mode = "zombie";
        if (!online) SceneManager.LoadScene(3);
    }

    public void Load()
    {
        string path = Application.persistentDataPath + "/save.txt";
        if (!File.Exists(path))
        {
            _loadText.SetActive(true);
            _loadText.GetComponent<Text>().text = "Nothing to load";
        }
        else
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string date = reader.ReadLine();
                string[] dsplit = date.Split(' ');
                if(dsplit.Length != 1)
                {
                    _loadText.SetActive(true);
                    _loadText.GetComponent<Text>().text = "Loading from : " + date;
                    load = true;
                    _loadButton.SetActive(false);
                    _launchButton.SetActive(true);
                }
                else
                {
                    _loadText.SetActive(true);
                    _loadText.GetComponent<Text>().text = "An error has occured";
                }
            }
        }       
    }

    public void LaunchLoad()
    {
        SceneManager.LoadScene(3);
    }

}
