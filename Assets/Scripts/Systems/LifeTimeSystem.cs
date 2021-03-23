using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class LifeTimeSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    private DynamicBuffer<Entity> _buffer;

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

        Entities.WithNone<PowerUpPrefab>().WithAll<BulletSourceData>().ForEach(
            (ref LifeTimeData lifeTime, in Entity thisEntity) =>
            {
                lifeTime.lifeTimeSeconds -= deltaTime;
                if (lifeTime.lifeTimeSeconds < 0)
                {
                    ecb.DestroyEntity(thisEntity);
                }
            }).Schedule();
        
        Entities.WithNone<PowerUpPrefab>().WithAll<PowerUpTag>().ForEach(
            (ref LifeTimeData lifeTime, in Entity thisEntity, in PowerUpParticleData powerUpParticleData) =>
            {
                lifeTime.lifeTimeSeconds -= deltaTime;
                if (lifeTime.lifeTimeSeconds < 0)
                {
                    Pooler.Instance.DeSpawn(powerUpParticleData.PowerUpParticle);
                    ecb.DestroyEntity(thisEntity);
                }
            }).WithoutBurst().Run();
        
        
        
        
        
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}
