using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zombieProp : MonoBehaviour
{
    public int chanceToSpawn = 50;
    public string zombiePath = "Prop/zombieProp";

    private void Start()
    {
        StartCoroutine(WaitForNavmeh());
    }

    private IEnumerator WaitForNavmeh()
    {
        yield return new WaitForSeconds(5f);

        if (PhotonNetwork.isMasterClient)
        {
            if (Random.Range(0, 100) < chanceToSpawn)
            {
                PhotonNetwork.Instantiate(zombiePath, transform.position, Quaternion.identity, 0);
            }
        }
    }
}
