
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject bigShipExplode;
    [SerializeField] private GameObject bulletFire;
    private Queue<GameObject> bulletFireQueue;
    [SerializeField] private int bulletFirePoolAmount = 20;
    [SerializeField] private GameObject playerShipExplode;
    [SerializeField] private GameObject enemyShipExplode;
    [SerializeField] private GameObject enemyHit;
    private Queue<GameObject> enemyHitQueue;
    [SerializeField] private GameObject enemyShipCreated;
    [SerializeField] private GameObject enemyBigShipCreated;
    
    // Start is called before the first frame update
    void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GeneralFireRateSystem>().OnBulletFire += AudioManagerOnFire;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<PlayerCollisionSystem>().OnPlayerShot += AudioManagerOnPlayerShipExplode;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GameWavesControlSystem>().OnEnemyShipCreated += AudioManagerOnEnemyShipCreated;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<GameWavesControlSystem>().OnEnemyBigShipCreated += AudioManagerOnEnemyBigShipCreated;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EnemyCollisionSystem>().OnEnemyHit += AudioManagerOnAsteroidExplode;
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EnemyCollisionSystem>().OnBigShipDestroyed += AudioManagerOnEnemyBIGShipExplode;
        
        enemyBigShipCreated.SetActive(false);
        enemyShipCreated.SetActive(false);
        bigShipExplode.SetActive(false);
        for (int i = 0; i < bulletFirePoolAmount; i++)
        {
            var bulletSound = Instantiate(bulletFire);
            bulletSound.SetActive(false);
            bulletFireQueue.Enqueue(bulletSound);
        }
        for (int i = 0; i < bulletFirePoolAmount; i++)
        {
            var enemyHitSound = Instantiate(enemyHit);
            enemyHitSound.SetActive(false);
            enemyHitQueue.Enqueue(enemyHitSound);
        }
        playerShipExplode.SetActive(false);

    }

    private void AudioManagerOnEnemyBigShipCreated(object sender, EventArgs e)
    {
        enemyBigShipCreated.SetActive(true);
        enemyBigShipCreated.GetComponent<AudioSource>().Play();
    }

    private void AudioManagerOnEnemyShipCreated(object sender, EventArgs e)
    {
        enemyShipCreated.SetActive(true);
        enemyShipCreated.GetComponent<AudioSource>().Play();
    }

    private void AudioManagerOnFire(object sender, EventArgs e)
    {
        var bulletSound = bulletFireQueue.Dequeue();
        bulletFireQueue.Enqueue(bulletSound);
        bulletSound.SetActive(true);
        bulletSound.GetComponent<AudioSource>().Play();
    }
    
    private void AudioManagerOnAsteroidExplode(object sender, EventArgs e)
    {
        var enemyHitSound = enemyHitQueue.Dequeue();
        enemyHitQueue.Enqueue(enemyHitSound);
        enemyHitSound.SetActive(true);
        enemyHitSound.GetComponent<AudioSource>().Play();
    }
    private void AudioManagerOnEnemyShipExplode(object sender, EventArgs e)
    {
        ///PULL FROM POOL AUDIOOBJECT
    }

    private void AudioManagerOnEnemyBIGShipExplode(object sender, EventArgs e)
    {
        bigShipExplode.SetActive(true);
        bigShipExplode.GetComponent<AudioSource>().Play();
    }

    private void AudioManagerOnPlayerShipExplode(object sender, EventArgs e)
    {
        ///PULL FROM POOL AUDIOOBJECT
    }
    

}
