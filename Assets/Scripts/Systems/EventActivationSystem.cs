using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class EventActivationSystem : SystemBase
{
    public EventHandler OnBulletFire;
    public EventHandler OnEnemyHit;
    public EventHandler OnBigUFODestroyed;
    public EventHandler OnPlayerShot;
    public EventHandler OnEnemyUFOCreated;
    public EventHandler OnEnemyBigUFOCreated;
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
        
        Entities.WithChangeFilter<OnUFOCreated>().ForEach((ref OnUFOCreated onEnemyShipCreated, in UFOTag ufoTag) =>
        {
            if (onEnemyShipCreated.Value)
            {
                if (ufoTag.IsBigUFO)
                {
                    OnEnemyBigUFOCreated(this, EventArgs.Empty);
                }
                else
                {
                    OnEnemyUFOCreated(this, EventArgs.Empty);   
                }
                onEnemyShipCreated.Value= false;
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
        
        Entities.WithChangeFilter<OnPlayerShot>().ForEach((ref OnPlayerShot onPlayerShot, 
            in OnHitParticlesGameObjectClass particlesObjectClass, in Translation trans) =>
        {
            if (onPlayerShot.value)
            {
                OnPlayerShot(this, EventArgs.Empty);
                onPlayerShot.value = false;
                Pooler.Instance.Spawn(particlesObjectClass.ParticlePrefabObject, trans.Value, quaternion.identity);
            }
        }).WithoutBurst().Run();
        
        
        Entities.WithChangeFilter<OnEnemyHit>().WithAll<UFOTag>().ForEach((ref OnDestroyed onDestroyed, ref OnEnemyHit onEnemyHit, in UFOTag ufoTag,
            in OnHitParticlesGameObjectClass particlesObjectClass, in UFOLivesData ufoLivesData, in Translation trans) =>
        {
            if (onEnemyHit.Value)
            {
                Pooler.Instance.Spawn(particlesObjectClass.ParticlePrefabObject, trans.Value, quaternion.identity);
                onEnemyHit.Value= false;

                if (ufoLivesData.CurrentLives - 1 <= 0)
                {
                    onDestroyed.Value = true;

                    if (ufoTag.IsBigUFO)
                    {
                        OnBigUFODestroyed(this, EventArgs.Empty);
                    }
                    else
                    {
                        OnEnemyHit(this, EventArgs.Empty);
                    }
                }
                else
                {
                    OnEnemyHit(this, EventArgs.Empty);
                }
            }
        }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<OnEnemyHit>().WithAll<AsteroidsTag>().ForEach((ref OnDestroyed onDestroyed, in OnEnemyHit onEnemyHit,
            in OnHitParticlesGameObjectClass particlesObjectClass, in Translation trans) =>
        {
            if (onEnemyHit.Value)
            {
                Pooler.Instance.Spawn(particlesObjectClass.ParticlePrefabObject, trans.Value, quaternion.identity);
                onDestroyed.Value = true;
                OnEnemyHit(this, EventArgs.Empty);
            }
        }).WithoutBurst().Run();


        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        
    }
}

