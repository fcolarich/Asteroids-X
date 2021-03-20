
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidWavesSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    private LevelManagerData levelManager;


    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var localLevelManager = levelManager;
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();

        Entities.ForEach((ref LevelManagerData levelManagerData) =>
            {
                if (levelManagerData.NewWave)
                {
                    levelManagerData.NewWave = false;
                    localLevelManager = levelManagerData;

                    for (int i = 0; i < levelManagerData.AmountToSpawn; i++)
                    {
                        var spawnLocation = Random.insideUnitCircle * 100;
                        var newEntity =ecb.Instantiate(levelManagerData.SpawnEntity);
                        ecb.SetComponent(newEntity, new Translation() {Value = new float3(spawnLocation.x,spawnLocation.y,0)});
                    }
                }
            }).Run();

            int destroyedAsteroidsAmount = 0;
        
            Entities.WithAll<SmallAsteroidDestroyedTag>().ForEach((ref Translation translation, in Rotation rotation) =>
            {
                destroyedAsteroidsAmount += 1;
            }).Run();
            
            _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
            
            Debug.Log(destroyedAsteroidsAmount);
            if (destroyedAsteroidsAmount >= (localLevelManager.CurrentWave*2 + localLevelManager.AmountToSpawn) * 4)
            {
                Debug.Log("START NEW WAVE PLEACE");
            }
            //IF SMALL ASTEROID COUNT = 4+2*WAVE NUMBER THEN WAVE FINISHED AND STARTS A NEW ONE
            //var wave = localLevelManager.CurrentWave;



        }
    }
