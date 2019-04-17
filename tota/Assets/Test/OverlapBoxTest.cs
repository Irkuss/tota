using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapBoxTest : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(1,1,1));
    }


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Check());
    }

    private IEnumerator Check()
    {
        while(true)
        {
            Collider[] coll = Physics.OverlapBox(transform.position, new Vector3(0.5f, 0.5f, 0.5f), transform.rotation);
            int length = coll.Length;
            bool check = length == 0;
            if (check)
            {
                Debug.Log("OverlapBoxTest: Empty space!");
            }
            else
            {
                Debug.Log("OverlapBoxTest: Filled space");
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
