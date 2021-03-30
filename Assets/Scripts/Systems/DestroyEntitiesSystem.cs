using Unity.Entities;
using Unity.Transforms;

public partial class DestroyEntitiesSystem : SystemBase
{
    private BeginFixedStepSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;

    protected override void OnCreate()
    {
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginFixedStepSimulationEntityCommandBufferSystem>();
    }

   
    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;

      
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();


        Entities.WithChangeFilter<OnDestroyed>()
            .ForEach((int entityInQueryIndex, Entity thisEntity, in OnDestroyed onDestroyed) =>
            {
                if (onDestroyed.Value)
                {
                    ecb.DestroyEntity(entityInQueryIndex, thisEntity);
                }
            }).ScheduleParallel();
      
        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}

