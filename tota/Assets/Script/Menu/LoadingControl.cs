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
