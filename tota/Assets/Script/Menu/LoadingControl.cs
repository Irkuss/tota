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

    private float currentAmount = 0;
    private float speed = 15;
    private float count = 0;

    // Update is called once per frame
    void Update()
    {
        if (currentAmount < 100 && count == 0 && false) //Tanguy: j'en ai marre d'attendre, bonsoir le merge
        {
            currentAmount += speed * Time.deltaTime;
        }
        else
        {
            if (count == 0 || true) //Tanguy: j'en ai marre d'attendre, bonsoir le merge
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
        //SceneManager.LoadScene(3);
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel(3); //tanguy: probleme de rpc peut etre resolu par ce changement
        }
    }
}
