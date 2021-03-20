using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;


public class ReactOnTriggerSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem _endSimulationECBSystem;


    protected override void OnCreate()
    {
        _endSimulationECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var ecb = _endSimulationECBSystem.CreateCommandBuffer();


        Entities.ForEach((Entity thisEntity,ref CollisionControlData collisionControlData, in Translation trans) => {
            if (collisionControlData.HasCollided)
            {
                if (EntityManager.HasComponent<AsteroidsTag>(thisEntity))
                {
                    if (EntityManager.Exists(collisionControlData.AffectedTarget))
                    {
                        var playerPointsData = EntityManager.GetComponentData<PlayerPointsData>(thisEntity);
                        var pointsToAdd = playerPointsData.points + EntityManager
                            .GetComponentData<PlayerPointsData>(collisionControlData.AffectedTarget).points;

                        ecb.SetComponent(collisionControlData.AffectedTarget,
                            new PlayerPointsData {points = pointsToAdd});
                    }

                    var spawnEntityData = EntityManager.GetComponentData<SpawnEntityData>(thisEntity); 
                    var spawnAmount = spawnEntityData.AmountToSpawn;
                    var entityToSpawn = spawnEntityData.SpawnEntity;
                    
                    for (int i = 0; i < spawnAmount; i++)
                    {
                        var newEntity = ecb.Instantiate(entityToSpawn);
                        ecb.SetComponent(newEntity, new Translation {Value = trans.Value});
                    }
                    //INSTANTIATE SPECIAL EFFECTS HERE
                }

                ecb.DestroyEntity(thisEntity);
            }
        }).WithoutBurst().Run();
        _endSimulationECBSystem.AddJobHandleForProducer(this.Dependency);
    }
    
}
