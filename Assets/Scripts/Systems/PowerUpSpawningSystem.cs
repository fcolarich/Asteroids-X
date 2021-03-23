using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;


public class PowerUpSpawningSystem : SystemBase
{
    EndFixedStepSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    private NativeArray<Entity> _powerUpArray;


    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }

    
    protected override void OnUpdate()
    {
        
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;

        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        var deltaTime = Time.DeltaTime;
        Entities.WithAll<PowerUpPrefab>().ForEach((Entity thisEntity,PowerUpParticleData powerUpParticleData, ref PowerUpData powerUpData, ref PowerUpPrefab powerUpPrefab) =>
      {
          powerUpPrefab.AppearanceTimer -= deltaTime;
          
          if (powerUpPrefab.AppearanceTimer < 0)
          {
              powerUpPrefab.AppearanceTimer = powerUpPrefab.AppearanceTimeSeconds;

                  if (Random.value > 0.5)
                  {
                      var spawnLocation = new float3(Random.insideUnitCircle * 180, -50);
                      var newEntity = ecb.Instantiate(thisEntity);
                      ecb.RemoveComponent<PowerUpPrefab>(newEntity);
                      ecb.SetComponent(newEntity, new Translation {Value = spawnLocation});
                      Object.Instantiate(powerUpParticleData.PowerUpParticle,spawnLocation, quaternion.identity);
                  }
          }
      }).WithoutBurst().Run(); 
        
       _powerUpArray = GetEntityQuery(ComponentType.ReadOnly<PowerUpPrefab>()).ToEntityArray(Allocator.Temp);

       Entities.WithAll<RandomPowerUpTag>().ForEach((Entity thisEntity, in Translation trans) =>
      {
          int value = Random.Range(0, _powerUpArray.Length);
          var newEntity = ecb.Instantiate(_powerUpArray[value]);
          ecb.RemoveComponent<PowerUpPrefab>(newEntity);
          ecb.AddComponent(newEntity, new ToInitializeTag());
          ecb.SetComponent(newEntity, new Translation {Value =  trans.Value});
          ecb.DestroyEntity(thisEntity);
      }).WithoutBurst().Run();
      
      Entities.WithAll<PowerUpTag>().WithAll<ToInitializeTag>().ForEach((Entity thisEntity, in Translation trans, in PowerUpParticleData powerUpParticleData) =>
      {
          ecb.RemoveComponent<ToInitializeTag>(thisEntity);
          var newParticles = Pooler.Instance.Spawn(powerUpParticleData.PowerUpParticle, trans.Value,Quaternion.identity);
          powerUpParticleData.PowerUpParticle = newParticles;
      }).WithoutBurst().Run();
      
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}
