using Unity.Entities;
using Unity.Physics;
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
        
        
        Entities.WithChangeFilter<OnCollision>().WithAll<PowerUpTag>()
            .ForEach((Entity thisEntity, ref PowerUpData powerUpEffectData, ref OnCollision onCollision,
                ref CollisionControlData collisionControlData, ref LifeTimeData lifeTimeData, ref OnDeactivateParticles onDeactivateParticles,
                in OnHitParticlesGameObjectClass particlesData, in GameObjectParticleData gameObjectParticleData) =>
            {
                if (onCollision.Value)
                {
                    onCollision.Value = false;
                    onDeactivateParticles.Value = true;
                    if (HasComponent<PlayerTag>(collisionControlData.AffectedTarget))
                    {
                        
                        ecb.RemoveComponent<PhysicsCollider>(thisEntity);
                        ecb.RemoveComponent<LifeTimeData>(thisEntity);
                        ecb.AddComponent(thisEntity, new ActivePowerUp());
                        
                        //We create the entity that will follow the target and will move the particle effect with the target
                        //We assign the target, then which particle effect to spawn and lastly the duration
                        //In another system, the entity will spawn the gameObject particle effect and in yet another one, move it around.
                        var affectedTarget = collisionControlData.AffectedTarget;
                        var linkedParticleEntity = ecb.CreateEntity();
                        ecb.AddComponent(linkedParticleEntity, new LinkedParticleData()
                        {
                            Target = affectedTarget,
                            ParticleObject = particlesData.ParticlePrefabObject,
                            TimerToDestroy = powerUpEffectData.PowerUpDurationSeconds
                        });
                        powerUpEffectData.PowerUpTimer = powerUpEffectData.PowerUpDurationSeconds + 0.001f;
                    }
                    else
                    {
                        ecb.DestroyEntity(thisEntity);
                    }
                }
            }).WithoutBurst().Run();
    
        Entities.WithAll<PowerUpEffectFireBoosterData>().WithAll<ActivePowerUp>().ForEach((Entity thisEntity,
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



        Entities.WithAll<PowerUpEffectEngineBoosterData>().WithAll<ActivePowerUp>().ForEach((Entity thisEntity,
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



        Entities.WithAll<PowerUpEffectShieldData>().WithAll<ActivePowerUp>().ForEach((Entity thisEntity,
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

        Entities.WithAll<PowerUpBulletData>().WithAll<ActivePowerUp>().ForEach((Entity thisEntity,
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

