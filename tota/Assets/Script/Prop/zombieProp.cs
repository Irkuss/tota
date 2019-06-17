using System.Collections;
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
        if(spawningAi)
        {
            if (PhotonNetwork.isMasterClient)
            {
                StartCoroutine(WaitAI());
            }
            //On ne détruit pas le spawner tout de suite (mais a la fin de la coroutine)
        }
        else
        {
            if (PhotonNetwork.isMasterClient)
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
            //On detruit le spawner
            Destroy(this.gameObject);
        }
    }

    private IEnumerator WaitAI()
    {
        Debug.Log("zombieProp: starting to wait to spawn ai for whatever reason");
        yield return new WaitForSeconds(3f);
        Debug.Log("zombieProp: actually spawning ai at " + transform.position);
        GameObject.Find("eCentralManager").GetComponent<CharaManager>().SpawnChara(transform.position, "", "");
        //Destruction du spawner une fois l'ai spawned
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        if (Mode.Instance.ShouldZombieSpawn)
        {
            Generator.onGenerationFinished -= InstantiateZombie;
        }
    }
}
