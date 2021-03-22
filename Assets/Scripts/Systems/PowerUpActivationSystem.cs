using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Random = UnityEngine.Random;


public class PowerUpActivationSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;


    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;

        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        var deltaTime = Time.DeltaTime;
        Entities.WithAll<PowerUpEffectFireBoosterData>().ForEach((Entity thisEntity,
            ref PowerUpEffectData powerUpEffectData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref LifeTimeData lifeTimeData,
            in PowerUpEffectFireBoosterData powerUpEffectFireBoosterData) =>
        {
            if (collisionControlData.HasCollided)
            {
                ecb.RemoveComponent<PhysicsCollider>(thisEntity);
                if (EntityManager.Exists(collisionControlData.AffectedTarget))
                {
                    var affectedTarget = collisionControlData.AffectedTarget;
                    var targetForward = EntityManager.GetComponentData<Rotation>(collisionControlData.AffectedTarget);
                    trans.Value =
                        EntityManager.GetComponentData<Translation>(collisionControlData.AffectedTarget).Value -
                        math.forward(targetForward.Value) * 20;
                    var bulletFireData = GetComponent<BulletFireData>(affectedTarget);

                    if (powerUpEffectData.PowerUpTimer == 0)
                    {
                        bulletFireData.SecondsBetweenBullets -= powerUpEffectFireBoosterData.FireRateSecondsReduction;
                        bulletFireData.SecondsBetweenBulletGroups -=
                            powerUpEffectFireBoosterData.FireRateSecondsReduction*4;
                        powerUpEffectData.PowerUpTimer = powerUpEffectData.PowerUpDurationSeconds;
                        ecb.SetComponent(affectedTarget, bulletFireData);
                        lifeTimeData.lifeTimeSeconds += powerUpEffectData.PowerUpDurationSeconds;
                    }
                    else if (powerUpEffectData.PowerUpTimer < 0)
                    {
                        bulletFireData.SecondsBetweenBullets += powerUpEffectFireBoosterData.FireRateSecondsReduction;
                        bulletFireData.SecondsBetweenBulletGroups +=
                            powerUpEffectFireBoosterData.FireRateSecondsReduction*4;
                        ecb.SetComponent(affectedTarget, bulletFireData);
                        ecb.DestroyEntity(thisEntity);
                    }
                }
                else
                {
                    ecb.DestroyEntity(thisEntity);
                }
                powerUpEffectData.PowerUpTimer -= deltaTime;
            }
        }).WithoutBurst().Run();
        
        Entities.WithAll<PowerUpEffectEngineBoosterData>().ForEach((Entity thisEntity,
            ref PowerUpEffectData powerUpEffectData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref LifeTimeData lifeTimeData,
            in PowerUpEffectEngineBoosterData powerUpEffectEngineBoosterData) =>
        {
            if (collisionControlData.HasCollided)
            {
                ecb.RemoveComponent<PhysicsCollider>(thisEntity);
                if (EntityManager.Exists(collisionControlData.AffectedTarget))
                {
                    var affectedTarget = collisionControlData.AffectedTarget;
                    var targetForward = EntityManager.GetComponentData<Rotation>(collisionControlData.AffectedTarget);
                    trans.Value =
                        EntityManager.GetComponentData<Translation>(collisionControlData.AffectedTarget).Value -
                        math.forward(targetForward.Value) * 20;
                    var moveAccelerationData = GetComponent<MoveAccelerationData>(affectedTarget);

                    if (powerUpEffectData.PowerUpTimer == 0)
                    {
                        moveAccelerationData.acceleration *= powerUpEffectEngineBoosterData.EngineBoosterMultiplier;
                        powerUpEffectData.PowerUpTimer = powerUpEffectData.PowerUpDurationSeconds;
                        ecb.SetComponent(affectedTarget, moveAccelerationData);
                        lifeTimeData.lifeTimeSeconds += powerUpEffectData.PowerUpDurationSeconds;
                    }
                    else if (powerUpEffectData.PowerUpTimer < 0)
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

                powerUpEffectData.PowerUpTimer -= deltaTime;
            }
        }).WithoutBurst().Run();
        
        Entities.WithAll<PowerUpEffectShieldData>().ForEach((Entity thisEntity,ref PowerUpEffectData powerUpEffectData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref LifeTimeData lifeTimeData) =>
        {
            if (collisionControlData.HasCollided)
            {
                ecb.RemoveComponent<PhysicsCollider>(thisEntity);
                if (EntityManager.Exists(collisionControlData.AffectedTarget))
                {
                    var affectedTarget = collisionControlData.AffectedTarget;
                    var targetForward = EntityManager.GetComponentData<Rotation>(collisionControlData.AffectedTarget);
                    trans.Value = EntityManager.GetComponentData<Translation>(collisionControlData.AffectedTarget).Value - math.forward(targetForward.Value)*20;
                    var playerLivesData = GetComponent<PlayerLivesData>(affectedTarget);
                    
                    if (powerUpEffectData.PowerUpTimer == 0)
                    {
                        playerLivesData.UpdateDelayTimer += powerUpEffectData.PowerUpDurationSeconds;
                        powerUpEffectData.PowerUpTimer = powerUpEffectData.PowerUpDurationSeconds;
                        ecb.SetComponent(affectedTarget, playerLivesData);
                        lifeTimeData.lifeTimeSeconds += powerUpEffectData.PowerUpDurationSeconds;
                    }
                    else if (powerUpEffectData.PowerUpTimer < 0)
                    {
                        ecb.DestroyEntity(thisEntity);
                    }
                }
                else
                {
                    ecb.DestroyEntity(thisEntity);
                }
                powerUpEffectData.PowerUpTimer -= deltaTime;
            }
        }).WithoutBurst().Run();
        
        
        Entities.WithAll<PowerUpBulletData>().ForEach((Entity thisEntity,ref PowerUpBulletData powerUpBulletData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref LifeTimeData lifeTimeData) =>
        {
            if (collisionControlData.HasCollided)
            {
                ecb.RemoveComponent<PhysicsCollider>(thisEntity);
                if (EntityManager.Exists(collisionControlData.AffectedTarget))
                {
                    var affectedTarget = collisionControlData.AffectedTarget;
                    var bulletFireData = GetComponent<BulletFireData>(affectedTarget);
                    var targetForward = EntityManager.GetComponentData<Rotation>(affectedTarget);
                    trans.Value = EntityManager.GetComponentData<Translation>(affectedTarget).Value - math.forward(targetForward.Value)*15;

                    if (powerUpBulletData.PowerUpTimer == 0)
                    {
                        powerUpBulletData.PowerUpTimer = powerUpBulletData.PowerUpDurationSeconds;
                        bulletFireData.BulletPrefab = powerUpBulletData.NewBulletEntity;
                        ecb.SetComponent(affectedTarget, bulletFireData);
                        lifeTimeData.lifeTimeSeconds += powerUpBulletData.PowerUpDurationSeconds;
                    }
                    else if (powerUpBulletData.PowerUpTimer < 0)
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
                powerUpBulletData.PowerUpTimer -= deltaTime;
            }
        }).WithoutBurst().Run();
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}

