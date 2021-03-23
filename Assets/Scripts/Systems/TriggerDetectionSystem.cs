using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class TriggerDetectionSystem : JobComponentSystem
{
    private BuildPhysicsWorld _buildPhysicsWorld;
    private StepPhysicsWorld _stepPhysicsWorld;
    private EndInitializationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
    
    
    protected override void OnCreate()
    {
        _buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }


    struct OnTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<AsteroidsTag> asteroidTag;
        [ReadOnly] public ComponentDataFromEntity<PlayerTag> playerTag;
        [ReadOnly] public ComponentDataFromEntity<UFOTag> ufoTag;
        [ReadOnly] public ComponentDataFromEntity<PowerUpTag> powerUpTag;
        [ReadOnly] public ComponentDataFromEntity<BulletSourceData> bulletSourceData;
        public ComponentDataFromEntity<CollisionControlData> collisionControlData;
        public EntityCommandBuffer ecb;

        public EntityManager entityManager;
        public CollisionControlData localCollisionControlData;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            bool powerUp = false;

            if (playerTag.HasComponent(entityA) || playerTag.HasComponent(entityB))
            {
                if (powerUpTag.HasComponent(entityA))
                {
                    localCollisionControlData.AffectedTarget = entityB;
                    collisionControlData[entityA] = localCollisionControlData;
                    ecb.AddComponent<HasCollidedTag>(entityA);
                    powerUp = true;
                }
                else if (powerUpTag.HasComponent(entityB))
                {
                    localCollisionControlData.AffectedTarget = entityA;
                    collisionControlData[entityB]= localCollisionControlData;
                    ecb.AddComponent<HasCollidedTag>(entityB);
                    powerUp = true;
                }
            }
            else if (asteroidTag.HasComponent(entityA) || ufoTag.HasComponent(entityA))
            {
                if (entityManager.Exists(entityB))
                {
                    if (entityManager.Exists(bulletSourceData[entityB].Source))
                    {
                        localCollisionControlData.AffectedTarget = bulletSourceData[entityB].Source;
                    }
                }
            }
            else if (asteroidTag.HasComponent(entityB) || ufoTag.HasComponent(entityB))
            {
                if (entityManager.Exists(entityA))
                {
                    if (entityManager.Exists(bulletSourceData[entityA].Source))
                    {
                        localCollisionControlData.AffectedTarget = bulletSourceData[entityA].Source;
                    }
                }
            }

            if (powerUp) return;

            if (entityManager.Exists(entityA))
            {
                collisionControlData[entityA] = localCollisionControlData;
                ecb.AddComponent<HasCollidedTag>(entityA);
            }

            if (entityManager.Exists(entityB))
            {
                collisionControlData[entityB] = localCollisionControlData;
                ecb.AddComponent<HasCollidedTag>(entityB);
            }
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {

        if (!HasSingleton<GameStateData>())
        {
            return new JobHandle();
        }
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing)
        {
            return new JobHandle();
        };

        var job = new OnTriggerJob
        {
            asteroidTag = GetComponentDataFromEntity<AsteroidsTag>(true),
            playerTag = GetComponentDataFromEntity<PlayerTag>(true),
            ufoTag = GetComponentDataFromEntity<UFOTag>(true),
            powerUpTag = GetComponentDataFromEntity<PowerUpTag>(true),
            bulletSourceData = GetComponentDataFromEntity<BulletSourceData>(true),
            entityManager = EntityManager,
            collisionControlData = GetComponentDataFromEntity<CollisionControlData>(false),
            ecb = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer()
        };
        
        JobHandle jobHandle = job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        jobHandle.Complete();
        return jobHandle;
    }
}
