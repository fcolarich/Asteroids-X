using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class BulletCollisionDetectionSystem : JobComponentSystem
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
        [ReadOnly] public ComponentDataFromEntity<AsteroidsTag> AsteroidTag;
        [ReadOnly] public ComponentDataFromEntity<UFOTag> UfoTag;
        [ReadOnly] public ComponentDataFromEntity<BulletSourceData> BulletSourceData;
        [ReadOnly] public ComponentDataFromEntity<PlayerBulletTag> PlayerBulletTag;
        public ComponentDataFromEntity<OnCollisionRegistered> OnCollisionRegistered;
        public ComponentDataFromEntity<PlayerPointsData> PlayerPointsData;
        public EntityCommandBuffer Ecb;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;

            if (PlayerBulletTag.HasComponent(entityA) || PlayerBulletTag.HasComponent(entityB))
            {
                Entity enemyEntity;
                Entity playerBullet;

                if (AsteroidTag.HasComponent(entityA) || UfoTag.HasComponent(entityA))
                {
                    enemyEntity = entityA;
                    playerBullet = entityB;
                }
                else
                {
                    enemyEntity = entityB;
                    playerBullet = entityA;
                }

                if (PlayerPointsData.HasComponent(BulletSourceData[playerBullet].Source))
                {
                    if (!OnCollisionRegistered[playerBullet].Value)
                    {
                        OnCollisionRegistered[playerBullet] = new OnCollisionRegistered {Value = true};
                        var localPlayerPointsData = PlayerPointsData[BulletSourceData[playerBullet].Source];
                        localPlayerPointsData.points += PlayerPointsData[enemyEntity].points;
                        PlayerPointsData[BulletSourceData[playerBullet].Source] = localPlayerPointsData;
                        Ecb.SetComponent(playerBullet, new OnDestroyed {Value = true});
                        Ecb.SetComponent(enemyEntity, new OnCollision {Value = true});
                    }
                }
            }
        }
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new OnTriggerJob
        {
            OnCollisionRegistered = GetComponentDataFromEntity<OnCollisionRegistered>(),
            AsteroidTag = GetComponentDataFromEntity<AsteroidsTag>(true),
            PlayerPointsData = GetComponentDataFromEntity<PlayerPointsData>(),
            UfoTag = GetComponentDataFromEntity<UFOTag>(true),
            PlayerBulletTag = GetComponentDataFromEntity<PlayerBulletTag>(true),
            BulletSourceData = GetComponentDataFromEntity<BulletSourceData>(true),
            Ecb = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer()
        };
        
        var jobHandle = job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        
        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        
        return jobHandle;
    }
}
