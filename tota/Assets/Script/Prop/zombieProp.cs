﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombieProp : MonoBehaviour
{
    public int chanceToSpawn = 50;
    public string path = "Prop/zombieProp";

    public bool spawningZombie = false;
    public bool spawningRat = false;
    public bool spawningAi = false;



    private void Start()
    {
        //StartCoroutine(WaitForNavmeh());


        if((Mode.Instance.ShouldZombieSpawn && spawningZombie)
            || (Mode.Instance.ShouldAiSpawn && spawningAi)
            || spawningRat)
        {
            Generator.onGenerationFinished += InstantiateZombie;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void InstantiateZombie()
    {
        if (PhotonNetwork.isMasterClient)
        {
            if(spawningAi)
            {
                StartCoroutine(WaitAI());
            }
            else
            {
                if (Random.Range(0, 100) < chanceToSpawn)
                {
                    Quaternion angle = Quaternion.Euler(0, Random.Range(0, 360), 0);

                    if (Mode.Instance.online) PhotonNetwork.Instantiate(path, transform.position, angle, 0);
                    else
                    {
                        Instantiate(Resources.Load<GameObject>(path), transform.position, angle);
                    }
                }
            }
        }
        Destroy(this.gameObject);
    }

    private IEnumerator WaitAI()
    {
        Debug.Log("REACHED WAIT AI");
        yield return new WaitForSeconds(5f);
        GameObject.Find("eCentralManager").GetComponent<CharaManager>().SpawnChara(transform.position, "", "");
    }

    private void OnDestroy()
    {
        if (Mode.Instance.ShouldZombieSpawn)
        {
            Generator.onGenerationFinished -= InstantiateZombie;
        }
    }
}
