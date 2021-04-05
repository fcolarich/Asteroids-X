using Unity.Entities;

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
        
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        var ecb2 = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        var deltaTime = Time.DeltaTime;

        Entities.WithAll<BulletSourceData>().ForEach(
            (int entityInQueryIndex, ref LifeTimeData lifeTime, in Entity thisEntity) =>
            {
                lifeTime.lifeTimeSeconds -= deltaTime;
                if (lifeTime.lifeTimeSeconds < 0)
                {
                    ecb2.DestroyEntity(entityInQueryIndex, thisEntity);
                }
            }).ScheduleParallel();

        var onDestroyed = GetComponentDataFromEntity<OnDestroyed>();
        var onDeactivateParticles = GetComponentDataFromEntity<OnDeactivateParticles>();
        
        Entities.WithNone<PowerUpPrefab>().WithAll<PowerUpTag>().ForEach(( 
            ref LifeTimeData lifeTime) =>
            {
                lifeTime.lifeTimeSeconds -= deltaTime;

            }).ScheduleParallel();
        
            
        Entities.WithNone<PowerUpPrefab>().WithAll<PowerUpTag>().ForEach(( 
            ref LifeTimeData lifeTime, in Entity thisEntity) =>
        {
            if (lifeTime.lifeTimeSeconds < 0)
            {
                if (onDeactivateParticles[thisEntity].Value)
                {
                    onDestroyed[thisEntity] = new OnDestroyed() {Value = true};
                }
                else if (!onDestroyed[thisEntity].Value)
                {
                    onDeactivateParticles[thisEntity] = new OnDeactivateParticles() {Value = true};
                }
            }
        }).Schedule();
   
        
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}
