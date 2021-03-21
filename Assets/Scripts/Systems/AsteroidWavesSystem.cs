
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using Random = UnityEngine.Random;

public class AsteroidWavesSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    private bool newWave = true;
    private int amountToSpawn;


    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        int tempAmountToSpawn = 0;

        if (newWave)
        {
            newWave = false;
            Entities.ForEach((ref LevelManagerData levelManagerData) =>
            {
                levelManagerData.CurrentWave += 1;


                tempAmountToSpawn = levelManagerData.StartingAmountToSpawn +
                                    (levelManagerData.CurrentWave * levelManagerData.IncrementPerWave);

                for (int i = 0; i < tempAmountToSpawn; i++)
                {
                    var spawnLocation = Random.insideUnitCircle * 1000;
                    var newEntity = ecb.Instantiate(levelManagerData.SpawnEntity);
                    ecb.SetComponent(newEntity,
                        new Translation() {Value = new float3(spawnLocation.x, spawnLocation.y, -50)});
                }
            }).Run();
            amountToSpawn = tempAmountToSpawn;
        }

        int destroyedAsteroidsAmount = 0;

        Entities.WithAll<SmallAsteroidDestroyedTag>().ForEach((Entity thisEntity) =>
            {
                destroyedAsteroidsAmount += 1;
            })
            .Run();

        Debug.Log(amountToSpawn);        
        if (destroyedAsteroidsAmount >= amountToSpawn * 4)
        {
           newWave = true;
           Entities.WithAll<SmallAsteroidDestroyedTag>().ForEach((Entity thisEntity) =>
               {
                   ecb.DestroyEntity(thisEntity);
               })
               .Run();
        }
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
    
}
