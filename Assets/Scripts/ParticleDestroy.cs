using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroy : MonoBehaviour
{

    private void Awake()
    {
        StartCoroutine(WaitParticleEnd());
    }

    IEnumerator WaitParticleEnd()
    {
        yield return new WaitForSeconds(5);
        Destroy(this.gameObject);
    }
}
