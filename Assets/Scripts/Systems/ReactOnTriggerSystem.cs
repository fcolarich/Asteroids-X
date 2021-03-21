using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class ReactOnTriggerSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;


    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        var deltaTime = Time.DeltaTime;
        
        
        
        Entities.WithChangeFilter<CollisionControlData>().WithAll<AsteroidsTag>().ForEach(
            (Entity thisEntity, ref CollisionControlData collisionControlData, in Translation trans,
                in PlayerPointsData playerPointsData) =>
            {
                if (collisionControlData.HasCollided)
                {
                    collisionControlData.HasCollided = false;

                    var targetEntity = collisionControlData.AffectedTarget;

                    if (EntityManager.Exists(targetEntity))
                    {
                        var currentPlayerPoints = EntityManager.GetComponentData<PlayerPointsData>(targetEntity).points;
                        AddPointsToPlayer(targetEntity, playerPointsData, ecb, currentPlayerPoints);
                    }

                    var spawnEntityData = EntityManager.GetComponentData<SpawnEntityData>(thisEntity);
                    SpawnEntities(spawnEntityData, trans, ecb);
                    //INSTANTIATE SPECIAL EFFECTS HERE
                    ecb.DestroyEntity(thisEntity);
                }
            }).WithoutBurst().Run();

        
        
        Entities.ForEach((Entity thisEntity, ref PlayerLivesData playerLivesData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref MoveSpeedData moveSpeedData) =>
        {
            if (collisionControlData.HasCollided)
            {
                collisionControlData.HasCollided = false;
                if (playerLivesData.UpdateDelayTimer <= 0)
                {
                    //INSTANTIATE SHIP EXPLOSION HERE
                    if (playerLivesData.CurrentLives < 1)
                    {
                        ecb.DestroyEntity(thisEntity);
                    }
                    else
                    {
                        playerLivesData.UpdateDelayTimer = playerLivesData.UpdateDelaySeconds;
                        trans.Value = playerLivesData.OriginPosition;
                        moveSpeedData.movementSpeed = 0;
                        playerLivesData.CurrentLives -= 1;
                    }
                }
            }
            playerLivesData.UpdateDelayTimer -= deltaTime;
        }).WithoutBurst().Run();
        
        
        
        Entities.WithAll<EnemyBulletTag>().ForEach((Entity thisEntity,
            in CollisionControlData collisionControlData) =>
        {
            if (collisionControlData.HasCollided)
            {
                ecb.DestroyEntity(thisEntity);
            }
        }).Schedule();
        
        Entities.WithAll<PlayerBulletTag>().ForEach((Entity thisEntity,
            in CollisionControlData collisionControlData) =>
        {
            if (collisionControlData.HasCollided)
            {
                ecb.DestroyEntity(thisEntity);
            }
        }).Schedule();
        
        Entities.WithAll<EnemyUFOTag>().ForEach((Entity thisEntity,
            in CollisionControlData collisionControlData) =>
        {
            if (collisionControlData.HasCollided)
            {
                ecb.DestroyEntity(thisEntity);
            }
        }).Schedule();
        
        
        
        
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        

    }
    
    
    
    static void AddPointsToPlayer(Entity targetEntity, PlayerPointsData playerPointsData, EntityCommandBuffer ecb, int playerPoints)
    {
        var pointsToAdd = playerPointsData.points + playerPoints;
        ecb.SetComponent(targetEntity,
            new PlayerPointsData {points = pointsToAdd});
    }
    
    
    static void SpawnEntities(SpawnEntityData spawnEntityData, Translation trans, EntityCommandBuffer ecb)
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

