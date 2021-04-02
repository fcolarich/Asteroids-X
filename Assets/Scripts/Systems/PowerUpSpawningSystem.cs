using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using Random = UnityEngine.Random;


public class PowerUpSpawningSystem : SystemBase
{
    EndFixedStepSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    private BlobAssetReference<BlobArray<Entity>> _powerUpArray;

    private Entity _gameManager;
    

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
        
        /*Entities.WithAll<PowerUpPrefab>().ForEach((Entity thisEntity, GameObjectParticleData powerUpParticleData, ref PowerUpData powerUpData, ref PowerUpPrefab powerUpPrefab) =>
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
                      var newParticles = Pooler.Instance.Spawn(powerUpParticleData.PowerUpParticle,spawnLocation, quaternion.identity);
                      ecb.SetComponent(newEntity, new GameObjectParticleData() {PowerUpParticle = newParticles});
                  }
          }
      }).WithoutBurst().Run();*/ 
        
        
       //_powerUpArray = GetEntityQuery(ComponentType.ReadOnly<PowerUpPrefab>()).ToEntityArray(Allocator.Temp);

       Entity[] localArray = new Entity[5];
       Entities.ForEach((in PowerUpArrayData powerUpArrayData) =>
       {
           localArray = new Entity[powerUpArrayData.PowerUpArray.Value.Length];
           for (int i = 0; i < powerUpArrayData.PowerUpArray.Value.Length; i++)
           {
               localArray[i] = powerUpArrayData.PowerUpArray.Value[i];
           }
       }).WithoutBurst().Run();

       Entities.WithAll<RandomPowerUpTag>().ForEach((Entity thisEntity, in Translation trans) =>
      {
          int value = Random.Range(0, localArray.Length);
          var newEntity = ecb.Instantiate(localArray[value]);
          ecb.RemoveComponent<PowerUpPrefab>(newEntity);
          ecb.AddComponent(newEntity, new ToInitialize());
          ecb.SetComponent(newEntity, new Translation {Value =  trans.Value});
          ecb.DestroyEntity(thisEntity);
      }).WithoutBurst().Run();
      
      Entities.WithAll<PowerUpTag>().WithAll<ToInitialize>().ForEach((Entity thisEntity, GameObjectParticleData powerUpParticleData, in Translation trans) =>
      {
          ecb.RemoveComponent<ToInitialize>(thisEntity);
          var newParticles = Pooler.Instance.Spawn(powerUpParticleData.PowerUpParticle, trans.Value,Quaternion.identity);
          ecb.SetComponent(thisEntity, new GameObjectParticleData() {PowerUpParticle = newParticles});

      }).WithoutBurst().Run();
      
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}
