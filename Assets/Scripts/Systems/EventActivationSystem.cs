using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EventActivationSystem : SystemBase
{
    public EventHandler OnBulletFire;
    public EventHandler OnEnemyHit;
    public EventHandler OnBigShipDestroyed;
    public EventHandler OnPlayerShot;
    public EventHandler OnEnemyShipCreated;
    public EventHandler OnEnemyBigShipCreated;
    public EventHandler OnPowerUpActivated;
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;

    protected override void OnCreate()
    {
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;

      
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();

        Entities.WithAll<LinkedParticleData>().WithNone<ParticleLinkTag>().ForEach((Entity thisEntity,LinkedParticleData linkedParticleData) =>
        {
            OnPowerUpActivated(this, EventArgs.Empty);
            var particleObject = Pooler.Instance.Spawn(linkedParticleData.ParticleObject);
            linkedParticleData.ParticleObject = particleObject;
            ecb.AddComponent(thisEntity, typeof(ParticleLinkTag));
        }).WithoutBurst().Run();
        
        
        Entities.WithChangeFilter<OnDeactivateParticles>().ForEach((GameObjectParticleData gameObjectParticleData , ref OnDeactivateParticles onDeactivateParticles) =>
        {
            if (onDeactivateParticles.Value)
            {
                Pooler.Instance.DeSpawn(gameObjectParticleData.ParticleGameObject);
            }
        }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<OnEnemyShipCreated>().ForEach((ref OnEnemyShipCreated onEnemyShipCreated) =>
        {
            if (onEnemyShipCreated.Value)
            {
                OnEnemyShipCreated(this, EventArgs.Empty);
                onEnemyShipCreated.Value= false;
            }
        }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<OnEnemyBigShipCreated>().ForEach((ref OnEnemyBigShipCreated onEnemyBigShipCreated) =>
        {
            if (onEnemyBigShipCreated.Value)
            {
                OnEnemyBigShipCreated(this, EventArgs.Empty);
                onEnemyBigShipCreated.Value= false;
            }
        }).WithoutBurst().Run();

        Entities.WithChangeFilter<OnBulletFired>().ForEach((ref OnBulletFired onBulletFired) =>
        {
            if (onBulletFired.Value)
            {
                OnBulletFire(onBulletFired, EventArgs.Empty);
                onBulletFired.Value= false;
            }
        }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<OnPlayerShot>().ForEach((ref OnPlayerShot onPlayerShot, in OnHitParticlesGameObjectClass particlesData, in Translation trans) =>
        {
            if (onPlayerShot.value)
            {
                OnPlayerShot(this, EventArgs.Empty);
                onPlayerShot.value = false;
                Pooler.Instance.Spawn(particlesData.ParticlePrefabObject, trans.Value, quaternion.identity);
            }
        }).WithoutBurst().Run();
        
        
        Entities.WithChangeFilter<OnEnemyHit>().WithNone<UFOBigTag>().ForEach((ref OnDestroyed onDestroyed, ref OnEnemyHit onEnemyHit, in OnHitParticlesGameObjectClass particlesData, in Translation trans) =>
        {
            if (onEnemyHit.Value)
            {
                OnEnemyHit(this, EventArgs.Empty);
                Pooler.Instance.Spawn(particlesData.ParticlePrefabObject, trans.Value, quaternion.identity);
                onEnemyHit.Value= false;
                onDestroyed.Value = true;
            }
        }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<OnEnemyHit>().WithAll<UFOBigTag>().ForEach((ref OnEnemyHit onEnemyHit, ref UFOLivesData ufoLivesData, in OnHitParticlesGameObjectClass particlesData, in Translation trans) =>
        {
            if (onEnemyHit.Value)
            {
                Pooler.Instance.Spawn(particlesData.ParticlePrefabObject, trans.Value, quaternion.identity);
                onEnemyHit.Value= false;
                if (ufoLivesData.CurrentLives - 1 < 0)
                {
                    OnBigShipDestroyed(this, EventArgs.Empty);
                }
                else
                {
                    OnEnemyHit(this, EventArgs.Empty);
                }
            }
        }).WithoutBurst().Run();
                            
        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        
    }
}

