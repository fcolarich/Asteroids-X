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


        Entities.WithAll<PowerUpTag>().WithAll<HasCollidedTag>()
            .ForEach((Entity thisEntity, ref PowerUpData powerUpEffectData,
                ref CollisionControlData collisionControlData, ref LifeTimeData lifeTimeData,
                in OnHitParticlesData particlesData, in PowerUpParticleData powerUpParticleData) =>
            {
                Object.DestroyImmediate(powerUpParticleData.PowerUpParticle);
                if (EntityManager.Exists(collisionControlData.AffectedTarget))
                {
                    ecb.RemoveComponent<PhysicsCollider>(thisEntity);
                    ecb.RemoveComponent<HasCollidedTag>(thisEntity);
                    ecb.AddComponent(thisEntity, new ActivePowerUpTag {});
                    var affectedTarget = collisionControlData.AffectedTarget;
                    var particleObject = Object.Instantiate(particlesData.ParticlePrefabObject);
                    var linkedParticleEntity = ecb.CreateEntity();
                    ecb.AddComponent(linkedParticleEntity, new LinkedParticleData()
                    {
                        Target = affectedTarget,
                        ParticleObject = particleObject,
                        TimerToDestroy = powerUpEffectData.PowerUpDurationSeconds
                    });
                    ecb.AddComponent(linkedParticleEntity, new ParticleLinkTag() { });
                    lifeTimeData.lifeTimeSeconds += powerUpEffectData.PowerUpDurationSeconds+0.001f;
                    powerUpEffectData.PowerUpTimer = powerUpEffectData.PowerUpDurationSeconds+0.001f;
                }
                else
                {                    
                    ecb.DestroyEntity(thisEntity);
                }
            }).WithoutBurst().Run();
    
    Entities.WithAll<PowerUpEffectFireBoosterData>().WithAll<ActivePowerUpTag>().ForEach((Entity thisEntity,
            ref PowerUpData powerUpData,
            ref CollisionControlData collisionControlData,
            in PowerUpEffectFireBoosterData powerUpEffectFireBoosterData) =>
        {
            if (EntityManager.Exists(collisionControlData.AffectedTarget))
            {
                var affectedTarget = collisionControlData.AffectedTarget;
                var bulletFireData = GetComponent<BulletFireData>(affectedTarget);

                if (powerUpData.PowerUpTimer >= powerUpData.PowerUpDurationSeconds)
                {
                    bulletFireData.SecondsBetweenBullets -= powerUpEffectFireBoosterData.FireRateSecondsReduction;
                    bulletFireData.SecondsBetweenBulletGroups -=
                        powerUpEffectFireBoosterData.FireRateSecondsReduction * 8;
                    ecb.SetComponent(affectedTarget, bulletFireData);
                }
                else if (powerUpData.PowerUpTimer < 0)
                {
                    bulletFireData.SecondsBetweenBullets += powerUpEffectFireBoosterData.FireRateSecondsReduction;
                    bulletFireData.SecondsBetweenBulletGroups +=
                        powerUpEffectFireBoosterData.FireRateSecondsReduction /8;
                    ecb.SetComponent(affectedTarget, bulletFireData);
                    ecb.DestroyEntity(thisEntity);
                }
            }
            else
            {
                ecb.DestroyEntity(thisEntity);
            }
            powerUpData.PowerUpTimer -= deltaTime;
        }).WithoutBurst().Run();


        Entities.WithAll<PowerUpEffectEngineBoosterData>().WithAll<ActivePowerUpTag>().ForEach((Entity thisEntity,
            ref PowerUpData powerUpData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref LifeTimeData lifeTimeData,
            in PowerUpEffectEngineBoosterData powerUpEffectEngineBoosterData) =>
        {
            if (EntityManager.Exists(collisionControlData.AffectedTarget))
            {
                var affectedTarget = collisionControlData.AffectedTarget;
                var moveAccelerationData = GetComponent<MoveAccelerationData>(affectedTarget);
                if (powerUpData.PowerUpTimer >= powerUpData.PowerUpDurationSeconds)
                {
                    moveAccelerationData.acceleration *= powerUpEffectEngineBoosterData.EngineBoosterMultiplier;
                    ecb.SetComponent(affectedTarget, moveAccelerationData);
                }
                else if (powerUpData.PowerUpTimer < 0)
                {
                    moveAccelerationData.acceleration /= powerUpEffectEngineBoosterData.EngineBoosterMultiplier;
                    ecb.SetComponent(affectedTarget, moveAccelerationData);
                    ecb.DestroyEntity(thisEntity);
                }
            }
            else
            {
                ecb.DestroyEntity(thisEntity);
            }
            powerUpData.PowerUpTimer -= deltaTime;
        }).WithoutBurst().Run();

        Entities.WithAll<PowerUpEffectShieldData>().WithAll<ActivePowerUpTag>().ForEach((Entity thisEntity,
            ref PowerUpData powerUpData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref LifeTimeData lifeTimeData) =>
        {
            if (EntityManager.Exists(collisionControlData.AffectedTarget))
            {
                var affectedTarget = collisionControlData.AffectedTarget;
                var playerLivesData = GetComponent<PlayerLivesData>(affectedTarget);
                if (powerUpData.PowerUpTimer >= powerUpData.PowerUpDurationSeconds)
                {
                    playerLivesData.CanTakeDamage = false;
                    ecb.SetComponent(affectedTarget, playerLivesData);
                }
                else if (powerUpData.PowerUpTimer < 0)
                {
                    playerLivesData.CanTakeDamage = true;
                    ecb.SetComponent(affectedTarget, playerLivesData);
                    ecb.DestroyEntity(thisEntity);
                }
            }
            else
            {
                ecb.DestroyEntity(thisEntity);
            }
            powerUpData.PowerUpTimer -= deltaTime;
        }).WithoutBurst().Run();


        Entities.WithAll<PowerUpBulletData>().WithAll<ActivePowerUpTag>().ForEach((Entity thisEntity,
            ref PowerUpBulletData powerUpBulletData,
            ref PowerUpData powerUpData, ref CollisionControlData collisionControlData, ref Translation trans,
            ref LifeTimeData lifeTimeData) =>
        {
            if (EntityManager.Exists(collisionControlData.AffectedTarget))
            {
                var affectedTarget = collisionControlData.AffectedTarget;
                var bulletFireData = GetComponent<BulletFireData>(affectedTarget);
                if (powerUpData.PowerUpTimer >= powerUpData.PowerUpDurationSeconds)
                {
                    bulletFireData.BulletPrefab = powerUpBulletData.NewBulletEntity;
                    ecb.SetComponent(affectedTarget, bulletFireData);
                }
                else if (powerUpData.PowerUpTimer < 0)
                {
                    bulletFireData.BulletPrefab = powerUpBulletData.OldBulletEntity;
                    ecb.SetComponent(affectedTarget, bulletFireData);
                    ecb.DestroyEntity(thisEntity);
                }
            }
            else
            {
                ecb.DestroyEntity(thisEntity);
            }
            powerUpData.PowerUpTimer -= deltaTime;
        }).WithoutBurst().Run();
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}

