using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class LifeTimeSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;


    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    
    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
        var deltaTime = Time.DeltaTime;

        Entities.WithNone<PowerUpPrefab>().ForEach(
            (int entityInQueryIndex, ref LifeTimeData lifeTime, in Entity thisEntity,
                in BulletSourceData bulletSource) =>
            {
                lifeTime.lifeTimeSeconds -= deltaTime;
                if (lifeTime.lifeTimeSeconds < 0)
                {
                    ecb.DestroyEntity(entityInQueryIndex, thisEntity);
                }
            }).ScheduleParallel();
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}
