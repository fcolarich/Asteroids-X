using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class PlayerCollisionDetectionSystem : JobComponentSystem
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
        [ReadOnly] public ComponentDataFromEntity<PlayerTag> PlayerTag;
        public ComponentDataFromEntity<OnCollisionRegistered> OnCollisionRegistered;
        [ReadOnly] public ComponentDataFromEntity<PowerUpTag> PowerUpTag;
        public ComponentDataFromEntity<CollisionControlData> CollisionControlData;
        public EntityCommandBuffer Ecb;

        public CollisionControlData LocalCollisionControlData;

        public void Execute(TriggerEvent triggerEvent)
        {
            var entityA = triggerEvent.EntityA;
            var entityB = triggerEvent.EntityB;
            var playerCollided = false;
            Entity playerEntity = entityA;
            Entity otherEntity = entityB;

            if (PlayerTag.HasComponent(entityA))
            {
                playerCollided = true;
            }
            else if (PlayerTag.HasComponent(entityB))
            {
                playerEntity = entityB;
                otherEntity = entityA;
                playerCollided = true;
            }

            if (playerCollided)
            {
                if (!OnCollisionRegistered[otherEntity].Value)
                {
                    OnCollisionRegistered[otherEntity] = new OnCollisionRegistered() {Value = true};

                    if (PowerUpTag.HasComponent(otherEntity))
                    {
                        LocalCollisionControlData.AffectedTarget = playerEntity;
                        CollisionControlData[otherEntity] = LocalCollisionControlData;
                        Ecb.SetComponent(otherEntity, new OnCollision() {Value = true});
                    }
                    else
                    {
                        Ecb.SetComponent(playerEntity, new OnCollision() {Value = true});
                        Ecb.SetComponent(otherEntity, new OnCollision() {Value = true});
                    }
                }
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
            PlayerTag = GetComponentDataFromEntity<PlayerTag>(true),
            OnCollisionRegistered = GetComponentDataFromEntity<OnCollisionRegistered>(),
            PowerUpTag = GetComponentDataFromEntity<PowerUpTag>(true),
            CollisionControlData = GetComponentDataFromEntity<CollisionControlData>(),
            Ecb = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer()
        };
        
        JobHandle jobHandle = job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
