using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;


public class PowerUpSpawningSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    private NativeArray<Entity> powerUpArray;


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
        Entities.WithAll<PowerUpPrefab>().ForEach((Entity thisEntity, ref PowerUpPrefab powerUpPrefab) =>
      {
          powerUpPrefab.AppearanceTimer -= deltaTime;
          
          if (powerUpPrefab.AppearanceTimer < 0)
          {
              powerUpPrefab.AppearanceTimer = powerUpPrefab.AppearanceTimeSeconds;

                  if (Random.value > 0.5)
                  {
                      var spawnLocation = Random.insideUnitCircle * 180;
                      var newEntity = ecb.Instantiate(thisEntity);
                      ecb.RemoveComponent<PowerUpPrefab>(newEntity);
                      ecb.SetComponent(newEntity, new Translation {Value = new float3(spawnLocation, -50)});
                  }
          }
      }).Run(); 
        
       powerUpArray = GetEntityQuery(ComponentType.ReadOnly<PowerUpPrefab>()).ToEntityArray(Allocator.Temp);
  
      Entities.WithAll<RandomPowerUpTag>().ForEach((Entity thisEntity, in Translation trans) =>
      {
          int value = Random.Range(0, powerUpArray.Length);
          var newEntity = ecb.Instantiate(powerUpArray[value]);
          ecb.RemoveComponent<PowerUpPrefab>(newEntity); 
          ecb.SetComponent(newEntity, new Translation {Value =  trans.Value});
          ecb.DestroyEntity(thisEntity);
      }).WithoutBurst().Run();
      
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}
