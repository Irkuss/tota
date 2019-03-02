using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingControl : MonoBehaviour
{
    [SerializeField]
    private Slider _sliderLoad;
    private Slider SliderLoad
    {
        get { return _sliderLoad; }
    }

    [SerializeField]
    private Text _loadingSlider;
    private Text LoadingSlider
    {
        get { return _loadingSlider; }
    }

    /*private float currentAmount = 0;
    private float speed = 15;
    private float count = 0;

    // Update is called once per frame
    void Update()
    {
        if (currentAmount < 100 && count == 0)
        {
            currentAmount += speed * Time.deltaTime;
        }
        else
        {
            if (count == 0)
            {
                Loadscene();
                count += 1;
            }
        }

        SliderLoad.value = (currentAmount / 100); 
        
        LoadingSlider.text = Mathf.RoundToInt(Mathf.Clamp(currentAmount, 0,100)).ToString() + "%";
    }

    private void Loadscene()
    {
        //PhotonNetwork.LoadLevel(3);
        SceneManager.LoadScene(3);
    }*/

    public void Start()
    {
        StartCoroutine(LoadAsynchronously(3));
    }

    IEnumerator LoadAsynchronously (int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);
            SliderLoad.value = progress;
            LoadingSlider.text = progress * 100f + "%";

            yield return null;
            
        }
    }
}
