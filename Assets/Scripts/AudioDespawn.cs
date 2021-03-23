using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDespawn : MonoBehaviour
{

    [SerializeField] private float time;

    void OnEnable()
    {
        StartCoroutine(DespawnTimer());
    }
    
    
    IEnumerator DespawnTimer()
    {
        yield return new WaitForSeconds(time);;
        Pooler.Instance.DeSpawn(this.gameObject);
    } 
    
}
