using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroy : MonoBehaviour
{

    private void OnEnable()
    {
        StartCoroutine(WaitParticleEnd());
    }

    IEnumerator WaitParticleEnd()
    {
        yield return new WaitForSeconds(10);
        Pooler.Instance.DeSpawn(this.gameObject);
    }
}
