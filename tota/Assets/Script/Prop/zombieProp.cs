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
        if (PhotonNetwork.isMasterClient && IsSpawningSomething() && Random.Range(0, 100) < chanceToSpawn)
        {
            Generator.onGenerationFinished += InstantiateOrgProp;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private bool IsSpawningSomething()
    {
        if(spawningRat)
        {
            return true;
        }

        if (Mode.Instance.ShouldAiSpawn && spawningAi)
        {
            return true;
        }

        if (Mode.Instance.ShouldZombieSpawn && spawningZombie)
        {
            return true;
        }

        return false;

        /*
            return ((Mode.Instance.ShouldZombieSpawn && spawningZombie)
            || (Mode.Instance.ShouldAiSpawn && spawningAi)
            || spawningRat);
        */
    }

    private void InstantiateOrgProp()
    {
        if(spawningAi)
        {
            StartCoroutine(WaitAI());
            
            //On ne détruit pas le spawner tout de suite (mais a la fin de la coroutine)
        }
        else
        {
            Quaternion angle = Quaternion.Euler(0, Random.Range(0, 360), 0);

            if(Mode.Instance.online)
            {
                PhotonNetwork.Instantiate(path, transform.position, angle, 0);
            }
            else
            {
                Instantiate(Resources.Load<GameObject>(path), transform.position, angle);
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
            Generator.onGenerationFinished -= InstantiateOrgProp;
        }
    }
}
