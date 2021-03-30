using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class EnemyCollisionSystem1 : JobComponentSystem
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
        [ReadOnly] public ComponentDataFromEntity<UFOTag> ufoTag;
        [ReadOnly] public ComponentDataFromEntity<BulletSourceData> bulletSourceData;
        public EntityCommandBuffer ecb;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;
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
            ufoTag = GetComponentDataFromEntity<UFOTag>(true),
            bulletSourceData = GetComponentDataFromEntity<BulletSourceData>(true),
            ecb = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer()
        };
        
        JobHandle jobHandle = job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
