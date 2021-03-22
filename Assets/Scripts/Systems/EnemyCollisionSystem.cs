using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;


public class EnemyCollisionSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    public EventHandler OnPointsUpdatePlayer1;
    public EventHandler OnPointsUpdatePlayer2;
    public EventHandler OnEnemyHit;
    public EventHandler OnBigShipDestroyed;




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
        
        Entities.WithChangeFilter<CollisionControlData>().WithAny<AsteroidsTag>().WithAny<UFOSmallTag>().WithAny<UFOMediumTag>().ForEach(
            (Entity thisEntity, ref CollisionControlData collisionControlData, in Translation trans,
                in PlayerPointsData playerPointsData, in SpawnEntityData spawnEntityData, in PowerUpRandomAppearData powerUpRandomAppearData) =>
            {
                if (collisionControlData.HasCollided)
                {
                    collisionControlData.HasCollided = false;
                    var targetEntity = collisionControlData.AffectedTarget;
                    

                    if (EntityManager.Exists(targetEntity))
                    {
                        var currentPlayerPoints = EntityManager.GetComponentData<PlayerPointsData>(targetEntity).points;
                        AddPointsToPlayer(targetEntity, playerPointsData, ecb, currentPlayerPoints);
                        if (HasComponent<Player1Tag>(targetEntity))
                        {
                            OnPointsUpdatePlayer1(playerPointsData.points+currentPlayerPoints, EventArgs.Empty);
                        }
                        else
                        {
                            OnPointsUpdatePlayer2(playerPointsData.points+currentPlayerPoints, EventArgs.Empty);
                        }
                    }
                    
                    SpawnEntities(spawnEntityData.AmountToSpawn, spawnEntityData.SpawnEntity, trans, ecb, true);
                    if (Random.value < powerUpRandomAppearData.AppearanceChance)
                    {
                        SpawnEntities(1, powerUpRandomAppearData.RandomPowerUp, trans, ecb, false);
                    }
                    
                    OnEnemyHit(this, EventArgs.Empty); //SAME AS WITH PLAYER, WE NEED TO SEND LOCATION TO SPAWN FX
                    
                    ecb.DestroyEntity(thisEntity);
                }
            }).WithoutBurst().Run();

        Entities.WithAll<UFOBigTag>().ForEach(
            (Entity thisEntity, ref UFOLivesData ufoLivesData, ref CollisionControlData collisionControlData, in Translation trans,
                in PlayerPointsData playerPointsData, in SpawnEntityData spawnEntityData) =>
            {
                if (collisionControlData.HasCollided)
                {
                    collisionControlData.HasCollided = false;
                    
                    if (ufoLivesData.UpdateDelayTimer <= 0)
                    {
                        var targetEntity = collisionControlData.AffectedTarget;

                        if (EntityManager.Exists(targetEntity))
                        {
                            var currentPlayerPoints =
                                EntityManager.GetComponentData<PlayerPointsData>(targetEntity).points;
                            AddPointsToPlayer(targetEntity, playerPointsData, ecb, currentPlayerPoints);
                            
                            if (HasComponent<Player1Tag>(targetEntity))
                            {
                                OnPointsUpdatePlayer1(playerPointsData.points+currentPlayerPoints, EventArgs.Empty);
                            }
                            else
                            {
                                OnPointsUpdatePlayer2(playerPointsData.points+currentPlayerPoints, EventArgs.Empty);
                            }
                        }

                        SpawnEntities(spawnEntityData.AmountToSpawn, spawnEntityData.SpawnEntity, trans, ecb, true);

                        OnEnemyHit(this, EventArgs.Empty); //SAME AS WITH PLAYER, WE NEED TO SEND LOCATION TO SPAWN FX
                        
                        var currentLives = ufoLivesData.CurrentLives - 1;
                        if (currentLives > 0)
                        {
                            ufoLivesData.CurrentLives = currentLives;
                            ufoLivesData.UpdateDelayTimer = ufoLivesData.UpdateDelaySeconds;
                        }
                        else
                        {
                            ecb.DestroyEntity(thisEntity);
                            OnBigShipDestroyed(this, EventArgs.Empty);//SAME AS WITH PLAYER, WE NEED TO SEND LOCATION TO SPAWN FX
                        }
                    }
                } 
                ufoLivesData.UpdateDelayTimer -= deltaTime;
            }).WithoutBurst().Run();


        Entities.WithAll<EnemyBulletTag>().ForEach((Entity thisEntity,
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
    
    static void SpawnEntities(int amountToSpawn, Entity entityToSpawn, Translation trans, EntityCommandBuffer ecb, bool SpawnInRandomPosition)
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            var newEntity = ecb.Instantiate(entityToSpawn);
            float2 spawnLocation = 0;
            if (SpawnInRandomPosition)
            {
               spawnLocation = Random.insideUnitCircle.normalized *10;    
            }

            ecb.SetComponent(newEntity, new Translation {Value = new float3(trans.Value.x +spawnLocation.x,
                trans.Value.y +spawnLocation.y,-50)});
        }
    }
    
    

}

