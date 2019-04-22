﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureManager : Interactable
{
    public Furniture furniture;

    [SerializeField] private Image fill = null;
    [SerializeField] private GameObject _drop = null;
    
    public override void Interact(CharaHead chara, int actionIndex)
    {
        Debug.Log("Interacting with furniture managere");
        fill.gameObject.SetActive(true);
        // utiliser les compétences pour pouvoir ouvrir

        while (fill.fillAmount < 1)
        {
            fill.fillAmount += Time.deltaTime / 3600;            
        }
        if (furniture.usable)
        {
            fill.color = Color.green;
            furniture.Interact(chara,GameObject.Find("eCentralManager").GetComponent<CentralManager>().InventoryLayout,this);
            fill.enabled = false;
            _drop.SetActive(true);
        }
        else
        {
            fill.color = Color.red;
        }
    }   
   
}
