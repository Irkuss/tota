using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instance.PlayAtPosition("Solitude", transform.position);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
