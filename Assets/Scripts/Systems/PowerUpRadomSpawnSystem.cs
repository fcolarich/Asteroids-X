using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class PowerUpRadomSpawnSystem : SystemBase
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
        var ecb2 = _endSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        var elapsedTime = Time.ElapsedTime;
        var deltaTime = Time.DeltaTime;

        var powerUpArrayEntity = GetSingletonEntity<PowerUpArrayData>();
        var powerUpArrayData = GetComponentDataFromEntity<PowerUpArrayData>(true)[powerUpArrayEntity];

        Entities.WithAll<RandomPowerUpTag>().ForEach((int entityInQueryIndex, Entity thisEntity, in Translation trans) =>
       {
          int value = Random.CreateFromIndex(Convert.ToUInt32((1+entityInQueryIndex)*elapsedTime)).NextInt(0, powerUpArrayData.PowerUpArray.Value.Length);
          var newEntity = ecb2.Instantiate(entityInQueryIndex,powerUpArrayData.PowerUpArray.Value[value]);
          ecb2.SetComponent(entityInQueryIndex,newEntity, new ToInitialize {Value = true});
          ecb2.SetComponent(entityInQueryIndex,newEntity, new Translation {Value =  trans.Value});
          ecb2.DestroyEntity(entityInQueryIndex,thisEntity);
      }).ScheduleParallel();
      
        Entities.ForEach((int entityInQueryIndex, ref PowerUpSpawnData powerUpSpawnData) =>
        {
         if (powerUpSpawnData.PowerUpTimer < 0)
         {
             powerUpSpawnData.PowerUpTimer = powerUpSpawnData.PowerUpSpawnIntervalSeconds;
             int value = Random.CreateFromIndex(Convert.ToUInt32((1+entityInQueryIndex)*elapsedTime)).NextInt(0, powerUpArrayData.PowerUpArray.Value.Length);
             var newEntity = ecb2.Instantiate(entityInQueryIndex,powerUpArrayData.PowerUpArray.Value[value]);
             ecb2.SetComponent(entityInQueryIndex,newEntity, new ToInitialize {Value = true});
             var index = Convert.ToUInt32(elapsedTime*deltaTime);
             var randomPointInCircleX = math.cos(Random.CreateFromIndex(index).NextFloat(360));
             var randomPointInCircleY = math.sin(Random.CreateFromIndex(index).NextFloat(360));
             var trans = new float2(randomPointInCircleX, randomPointInCircleY)*50;
             ecb2.SetComponent(entityInQueryIndex,newEntity, new Translation {Value =  new float3(trans, -50)});

         }
         else
         {
             powerUpSpawnData.PowerUpTimer -= deltaTime;
         }
        }).ScheduleParallel();
        
        
        
        
      Entities.WithAll<PowerUpTag>().WithChangeFilter<ToInitialize>().ForEach((Entity thisEntity, GameObjectParticleData powerUpParticleData,ref ToInitialize toInitialize,in Translation trans) =>
      {
          if (toInitialize.Value)
          {
              toInitialize.Value = false;
              var newParticles =
                  Pooler.Instance.Spawn(powerUpParticleData.ParticleGameObject, trans.Value, Quaternion.identity);
              ecb.SetComponent(thisEntity, new GameObjectParticleData() {ParticleGameObject = newParticles});
          }
      }).WithoutBurst().Run();

        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}
