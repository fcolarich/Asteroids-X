using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


public class PowerUpActivationSystem : SystemBase
{
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

        var deltaTime = Time.DeltaTime;
        
      
        var _moveAccelerationData = GetComponentDataFromEntity<MoveAccelerationData>();
        var _playerLivesData = GetComponentDataFromEntity<PlayerLivesData>();
        var _bulletFireData = GetComponentDataFromEntity<BulletFireData>();
        
        
        Entities.WithChangeFilter<OnCollision>().WithAll<PowerUpTag>().WithNone<PowerUpPrefab>()
            .ForEach((Entity thisEntity, ref PowerUpData powerUpEffectData, ref OnCollision onCollision,
                ref CollisionControlData collisionControlData, ref LifeTimeData lifeTimeData,
                in OnHitParticlesData particlesData, in GameObjectParticleData powerUpParticleData) =>
            {
                if (onCollision.Value)
                {
                    onCollision.Value = false;
                    
                    //DEACTIVATE PARTICLE - CHANGE THIS
                    powerUpParticleData.PowerUpParticle.SetActive(false);
                    
                    
                    if (HasComponent<PlayerTag>(collisionControlData.AffectedTarget))
                    {
                        // DEACTIVATE COLLIDER - CHANGE THIS WITHOUT REMOVING COMPONENT
                        ecb.RemoveComponent<PhysicsCollider>(thisEntity);
                        
                        // ACTIVATE POWERUP TAG TO START COUNTING TIME FOR POWER UP AND APPLY EFFECTS IN OTHER WITHALL
                        ecb.AddComponent(thisEntity, new OnActivePowerUp { });
                        var affectedTarget = collisionControlData.AffectedTarget;
                        var particleObject = Pooler.Instance.Spawn(particlesData.ParticlePrefabObject);
                        var linkedParticleEntity = ecb.CreateEntity();
                        ecb.AddComponent(linkedParticleEntity, new LinkedParticleData()
                        {
                            Target = affectedTarget,
                            ParticleObject = particleObject,
                            TimerToDestroy = powerUpEffectData.PowerUpDurationSeconds
                        });
                        ecb.AddComponent(linkedParticleEntity, new ParticleLinkTag() { });
                        lifeTimeData.lifeTimeSeconds += powerUpEffectData.PowerUpDurationSeconds + 0.001f;
                        powerUpEffectData.PowerUpTimer = powerUpEffectData.PowerUpDurationSeconds + 0.001f;
                    }
                    else
                    {
                        ecb.DestroyEntity(thisEntity);
                    }
                }
            }).WithoutBurst().Run();
    
        Entities.WithAll<PowerUpEffectFireBoosterData>().WithAll<OnActivePowerUp>().ForEach((Entity thisEntity,
            ref PowerUpData powerUpData,
            ref CollisionControlData collisionControlData,
            in PowerUpEffectFireBoosterData powerUpEffectFireBoosterData) =>
        {
            if (HasComponent<PlayerTag>(collisionControlData.AffectedTarget))
            {
                var affectedTarget = collisionControlData.AffectedTarget;
                var bulletFireData = _bulletFireData[affectedTarget];

                if (powerUpData.PowerUpTimer >= powerUpData.PowerUpDurationSeconds)
                {
                    bulletFireData.SecondsBetweenBullets -= powerUpEffectFireBoosterData.FireRateSecondsReduction;
                    bulletFireData.SecondsBetweenBulletGroups -=
                        powerUpEffectFireBoosterData.FireRateSecondsReduction * 8;
                    _bulletFireData[affectedTarget] = bulletFireData;
                }
                else if (powerUpData.PowerUpTimer < 0)
                {
                    bulletFireData.SecondsBetweenBullets += powerUpEffectFireBoosterData.FireRateSecondsReduction;
                    bulletFireData.SecondsBetweenBulletGroups +=
                        powerUpEffectFireBoosterData.FireRateSecondsReduction /8;
                    _bulletFireData[affectedTarget] = bulletFireData;
                    ecb.DestroyEntity(thisEntity);
                }
            }
            else
            {
                ecb.DestroyEntity(thisEntity);
            }
            powerUpData.PowerUpTimer -= deltaTime;
        }).Schedule();



        Entities.WithAll<PowerUpEffectEngineBoosterData>().WithAll<OnActivePowerUp>().ForEach((Entity thisEntity,
            ref PowerUpData powerUpData, in CollisionControlData collisionControlData, 
            in PowerUpEffectEngineBoosterData powerUpEffectEngineBoosterData) =>
        {
            if (HasComponent<PlayerTag>(collisionControlData.AffectedTarget))
            {
                var affectedTarget = collisionControlData.AffectedTarget;
                var moveAccelerationData = _moveAccelerationData[affectedTarget];
                if (powerUpData.PowerUpTimer >= powerUpData.PowerUpDurationSeconds)
                {
                    moveAccelerationData.acceleration *= powerUpEffectEngineBoosterData.EngineBoosterMultiplier;
                    _moveAccelerationData[affectedTarget] = moveAccelerationData;
                }
                else if (powerUpData.PowerUpTimer < 0)
                {
                    moveAccelerationData.acceleration /= powerUpEffectEngineBoosterData.EngineBoosterMultiplier;
                    _moveAccelerationData[affectedTarget] = moveAccelerationData;
                    ecb.DestroyEntity(thisEntity);
                }
            }
            else
            {
                ecb.DestroyEntity(thisEntity);
            }
            powerUpData.PowerUpTimer -= deltaTime;
        }).Schedule();



        Entities.WithAll<PowerUpEffectShieldData>().WithAll<OnActivePowerUp>().ForEach((Entity thisEntity,
            ref PowerUpData powerUpData, in CollisionControlData collisionControlData) =>
        {
            if (HasComponent<PlayerTag>(collisionControlData.AffectedTarget))
            {
                var affectedTarget = collisionControlData.AffectedTarget;
                var playerLivesData = _playerLivesData[affectedTarget];
                if (powerUpData.PowerUpTimer >= powerUpData.PowerUpDurationSeconds)
                {
                    playerLivesData.CanTakeDamage = false;
                    _playerLivesData[affectedTarget] = playerLivesData;
                }
                else if (powerUpData.PowerUpTimer < 0)
                {
                    playerLivesData.CanTakeDamage = true;
                    _playerLivesData[affectedTarget] = playerLivesData;
                    ecb.DestroyEntity(thisEntity);
                }
            }
            else
            {
                ecb.DestroyEntity(thisEntity);
            }
            powerUpData.PowerUpTimer -= deltaTime;
        }).Schedule();

        Entities.WithAll<PowerUpBulletData>().WithAll<OnActivePowerUp>().ForEach((Entity thisEntity,
            ref PowerUpBulletData powerUpBulletData,
            ref PowerUpData powerUpData, in CollisionControlData collisionControlData) =>
        {
            if (HasComponent<PlayerTag>(collisionControlData.AffectedTarget))
            {
                var affectedTarget = collisionControlData.AffectedTarget;
                var bulletFireData = _bulletFireData[affectedTarget];
                if (powerUpData.PowerUpTimer >= powerUpData.PowerUpDurationSeconds)
                {
                    bulletFireData.BulletPrefab = powerUpBulletData.NewBulletEntity;
                    _bulletFireData[affectedTarget] =  bulletFireData;
                }
                else if (powerUpData.PowerUpTimer < 0)
                {
                    bulletFireData.BulletPrefab = powerUpBulletData.OldBulletEntity;
                    _bulletFireData[affectedTarget] =  bulletFireData;
                    ecb.DestroyEntity(thisEntity);
                }
            }
            else
            {
                ecb.DestroyEntity(thisEntity);
            }
            powerUpData.PowerUpTimer -= deltaTime;
        }).Schedule();
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}

