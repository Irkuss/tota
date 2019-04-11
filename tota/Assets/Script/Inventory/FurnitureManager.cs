using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureManager : Interactable
{
    public Furniture furniture;

    [SerializeField] private Image fill = null;

    public override void Interact(CharaHead chara)
    {
        fill.gameObject.SetActive(true);
        // utiliser les compétences pour pouvoir ouvrir

        while (fill.fillAmount < 1)
        {
            fill.fillAmount += Time.deltaTime;            
        }
        furniture.Interact();
    }   
   
}
