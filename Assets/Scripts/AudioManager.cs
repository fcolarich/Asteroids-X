
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject bigShipExplode;
    [SerializeField] private GameObject bulletFire;
    [SerializeField] private GameObject playerShipExplode;
    [SerializeField] private GameObject enemyShipExplode;
    [SerializeField] private GameObject enemyHit;
    [SerializeField] private GameObject enemyShipCreated;
    [SerializeField] private GameObject enemyBigShipCreated;
    [SerializeField] private GameObject music;
    [SerializeField] private GameObject player2Joined;
    private Pooler _pooler;


    void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventActivateSystem>().OnBulletFire += AudioManagerOnFire;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventActivateSystem>().OnPlayerShot += AudioManagerOnPlayerShipExplode;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GameWavesControlSystem>().OnEnemyShipCreated += AudioManagerOnEnemyShipCreated;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GameWavesControlSystem>().OnEnemyBigShipCreated += AudioManagerOnEnemyBigShipCreated;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventActivateSystem>().OnEnemyHit += AudioManagerOnEnemyExplode;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EventActivateSystem>().OnBigShipDestroyed += AudioManagerOnEnemyBIGShipExplode;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnRestart += AudioManagerOnRestart;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnPlayer2Join += AudioManagerOnPlayer2Join;
        _pooler = Pooler.Instance;
        StartCoroutine(MusicFadeIn());
    }

    IEnumerator MusicFadeIn()
    {
        var audioSource = music.GetComponent<AudioSource>(); 
        music.SetActive(true);
        audioSource.volume = 0;
        var fadingIn = true;
        while (fadingIn)
        {
            yield return new WaitForFixedUpdate();
            audioSource.volume += 0.001f;
            if (audioSource.volume >= 0.5f)
            {
                fadingIn = false;
            }
        }
    }
    

  
    
    private void AudioManagerOnPlayer2Join(object sender, EventArgs e)
    {
        _pooler.Spawn(player2Joined);
        
        }

    private void AudioManagerOnRestart(object sender, EventArgs e)
    {
        foreach
            (var audio in FindObjectsOfType<AudioSource>())
        {
            _pooler.DeSpawn(audio.gameObject);   
        }  music.SetActive(true);
    }
    
    private void AudioManagerOnEnemyBigShipCreated(object sender, EventArgs e)
    {
        _pooler.Spawn(enemyBigShipCreated);
    }

    private void AudioManagerOnEnemyShipCreated(object sender, EventArgs e)
    {
        _pooler.Spawn(enemyShipCreated);
    }

    private void AudioManagerOnFire(object sender, EventArgs e)
    {
        _pooler.Spawn(bulletFire);
    }
    
    private void AudioManagerOnEnemyExplode(object sender, EventArgs e)
    {
        _pooler.Spawn(enemyHit);
    }
    private void AudioManagerOnEnemyShipExplode(object sender, EventArgs e)
    {
        _pooler.Spawn(enemyShipExplode);
    }

    private void AudioManagerOnEnemyBIGShipExplode(object sender, EventArgs e)
    {
        _pooler.Spawn(bigShipExplode);
    }

    private void AudioManagerOnPlayerShipExplode(object sender, EventArgs e)
    {
        _pooler.Spawn(playerShipExplode);
    }
    

}
