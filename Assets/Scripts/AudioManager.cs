
using System;
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
    private Pooler _pooler;


    void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GeneralFireRateSystem>().OnBulletFire += AudioManagerOnFire;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerCollisionSystem>().OnPlayerShot += AudioManagerOnPlayerShipExplode;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GameWavesControlSystem>().OnEnemyShipCreated += AudioManagerOnEnemyShipCreated;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GameWavesControlSystem>().OnEnemyBigShipCreated += AudioManagerOnEnemyBigShipCreated;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EnemyCollisionSystem>().OnEnemyHit += AudioManagerOnAsteroidExplode;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EnemyCollisionSystem>().OnBigShipDestroyed += AudioManagerOnEnemyBIGShipExplode;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerInputSystem>().OnRestart += AudioManagerOnRestart;
        _pooler = Pooler.Instance;
        
    }

    private void AudioManagerOnRestart(object sender, EventArgs e)
    {
        foreach
            (var audio in FindObjectsOfType<AudioSource>())
        {
            _pooler.DeSpawn(audio.gameObject);   
        }    
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
    
    private void AudioManagerOnAsteroidExplode(object sender, EventArgs e)
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
