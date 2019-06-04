using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombieProp : MonoBehaviour
{
    public int chanceToSpawn = 50;
    public string path = "Prop/zombieProp";

    

    private void Start()
    {
        //StartCoroutine(WaitForNavmeh());

        Generator.onGenerationFinished += InstantiateZombie;
    }

    private IEnumerator WaitForNavmeh()
    {
        yield return new WaitForSeconds(3f);

        InstantiateZombie();
    }

    private void InstantiateZombie()
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
        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        Generator.onGenerationFinished -= InstantiateZombie;
    }
}
