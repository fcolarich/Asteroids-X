using Unity.Entities;
using Unity.Mathematics;
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
                    var targetEntity = collisionControlData.AffectedTarget;
                    
                    if (EntityManager.Exists(targetEntity))
                    {
                        var playerPointsData = EntityManager.GetComponentData<PlayerPointsData>(thisEntity);
                        var currentPlayerPoints = EntityManager.GetComponentData<PlayerPointsData>(targetEntity).points;
                        AddPointsToPlayer(targetEntity,playerPointsData,ecb,currentPlayerPoints);
                    }
                    
                    var spawnEntityData = EntityManager.GetComponentData<SpawnEntityData>(thisEntity); 
                    SpawnEntities(thisEntity, spawnEntityData, trans, ecb);
                    //INSTANTIATE SPECIAL EFFECTS HERE
                }
                ecb.DestroyEntity(thisEntity);
            }
        }).WithoutBurst().Run();
        _endSimulationECBSystem.AddJobHandleForProducer(this.Dependency);
    }
    
    
    
    static void AddPointsToPlayer(Entity targetEntity, PlayerPointsData playerPointsData, EntityCommandBuffer ecb, int playerPoints)
    {
        var pointsToAdd = playerPointsData.points + playerPoints;
        ecb.SetComponent(targetEntity,
            new PlayerPointsData {points = pointsToAdd});
    }
    
    
    static void SpawnEntities(Entity thisEntity, SpawnEntityData spawnEntityData, Translation trans, EntityCommandBuffer ecb)
    {
        var spawnAmount = spawnEntityData.AmountToSpawn;
        var entityToSpawn = spawnEntityData.SpawnEntity;
                    
        for (int i = 0; i < spawnAmount; i++)
        {
            var newEntity = ecb.Instantiate(entityToSpawn);
            ecb.SetComponent(newEntity, new Translation {Value = trans.Value});
        }
    }
    
    

}

