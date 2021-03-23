using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ParticleCleanup : MonoBehaviour
{
    
    
    void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnPause += ParticleManagerOnPause;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnResume += ParticleManagerOnResume;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnRestart += ParticleManagerOnRestart;
    }

    private void ParticleManagerOnRestart(object sender, EventArgs e)
    {
        foreach (var particles in FindObjectsOfType<ParticleSystem>())
        {
         Pooler.Instance.DeSpawn(particles.gameObject);   
        }
    }
    
    private void ParticleManagerOnPause(object sender, EventArgs e)
    {
        Time.timeScale = 0;
    }

    private void ParticleManagerOnResume(object sender, EventArgs e)
    {
        Time.timeScale = 1;
    }

}
