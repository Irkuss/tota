using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Solo()
    {
        SceneManager.LoadScene(2); // METTRE L'INDEX DU SOLO OU "SOLO"
    }

    public void Multiplayer()
    {
        SceneManager.LoadScene(1); // METTRE L'INDEX DU MULTI OU "MULTIJOUEUR"
    }

    public void Credits()
    {
        // LE MIEUX EST DE FAIRE UN PANEL AVEC UNE ANIMATION
    }

    
}
