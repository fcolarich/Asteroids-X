using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDespawn : MonoBehaviour
{

    [SerializeField] private float secondsUntilDeSpawn = 2f;
    
    void OnEnable()
    {
        StartCoroutine(DespawnAfterTime());
    }

    IEnumerator DespawnAfterTime()
    {
        yield return new WaitForSeconds(secondsUntilDeSpawn);
        Pooler.Instance.DeSpawn(this.gameObject);
    }
}
