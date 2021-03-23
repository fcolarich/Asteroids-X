using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDespawn : MonoBehaviour
{

    private WaitForSeconds _waitForSeconds;
    
    private void Start()
    {
        var time = this.GetComponent<AudioSource>().clip.length;
        _waitForSeconds = new WaitForSeconds(time);
    }

    void OnEnable()
    {
        StartCoroutine(DespawnTimer());
    }
    
    
    IEnumerator DespawnTimer()
    {
        yield return _waitForSeconds;
        Pooler.Instance.DeSpawn(this.gameObject);
    } 
    
}
