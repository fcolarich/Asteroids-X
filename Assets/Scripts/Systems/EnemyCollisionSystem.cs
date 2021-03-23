using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using Random = UnityEngine.Random;

public class EnemyCollisionSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;
    public EventHandler OnPointsUpdatePlayer1;
    public EventHandler OnPointsUpdatePlayer2;
    public EventHandler OnEnemyHit;
    public EventHandler OnBigShipDestroyed;

    protected override void OnCreate()
    {
        
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;

        
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();
        
        Entities.WithChangeFilter<CollisionControlData>().WithAll<HasCollidedTag>().WithAny<AsteroidsTag>().WithAny<UFOSmallTag>().WithAny<UFOMediumTag>().ForEach(
            (Entity thisEntity, ref CollisionControlData collisionControlData, in Translation trans,
                in PlayerPointsData playerPointsData, in SpawnEntityData spawnEntityData, in PowerUpRandomAppearData powerUpRandomAppearData, in OnHitParticlesData particlesData) =>
            {
                ecb.RemoveComponent<HasCollidedTag>(thisEntity);
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

                    OnEnemyHit(this, EventArgs.Empty);
                    Pooler.Instance.Spawn(particlesData.ParticlePrefabObject,trans.Value,quaternion.identity);
                    ecb.DestroyEntity(thisEntity);
                    
            }).WithoutBurst().Run();

        Entities.WithAll<UFOBigTag>().WithAll<HasCollidedTag>().ForEach(
            (Entity thisEntity, ref UFOLivesData ufoLivesData, ref CollisionControlData collisionControlData, in Translation trans,
                in PlayerPointsData playerPointsData, in SpawnEntityData spawnEntityData, in OnHitParticlesData particlesData) =>
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

                        OnEnemyHit(this, EventArgs.Empty);
                        Pooler.Instance.Spawn(particlesData.ParticlePrefabObject,trans.Value,quaternion.identity);

                        var currentLives = ufoLivesData.CurrentLives - 1;
                        if (currentLives > 0)
                        {
                            ufoLivesData.CurrentLives = currentLives;
                        }
                        else
                        {
                            ecb.DestroyEntity(thisEntity);
                            OnBigShipDestroyed(this, EventArgs.Empty);
                        }
                    ecb.RemoveComponent<HasCollidedTag>(thisEntity);
            }).WithoutBurst().Run();


        Entities.WithAll<EnemyBulletTag>().WithAll<HasCollidedTag>().ForEach((Entity thisEntity,
            in CollisionControlData collisionControlData,in Translation trans) =>
        {
            ecb.DestroyEntity(thisEntity);
        }).WithoutBurst().Run();
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
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

