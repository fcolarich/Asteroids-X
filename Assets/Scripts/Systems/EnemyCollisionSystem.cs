using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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

        var localCurrentPlayerPoints = GetComponentDataFromEntity<PlayerPointsData>(); 
        var localUfoLivesData = GetComponentDataFromEntity<UFOLivesData>(); 

        
        Entities.WithChangeFilter<CollisionControlData>().WithAll<HasCollidedTag>().WithAny<AsteroidsTag>()
            .WithAny<UFOSmallTag>().WithAny<UFOMediumTag>().WithAny<UFOBigTag>().ForEach(
                (Entity thisEntity, ref CollisionControlData collisionControlData, in Translation trans,
                    in PlayerPointsData playerPointsData, in SpawnEntityData spawnEntityData,
                    in PowerUpRandomAppearData powerUpRandomAppearData, in OnHitParticlesData particlesData) =>
                {
                    
                    ecb.RemoveComponent<HasCollidedTag>(thisEntity);
                    var targetEntity = collisionControlData.AffectedTarget;
                    if (HasComponent<PlayerTag>(collisionControlData.AffectedTarget))
                    {
                       var currentPlayerPoints = localCurrentPlayerPoints[targetEntity].points;
                       var pointsToAdd =  playerPointsData.points + currentPlayerPoints;
                       localCurrentPlayerPoints[targetEntity] = new PlayerPointsData(){points = pointsToAdd};

                       if (HasComponent<Player1Tag>(targetEntity))
                        {
                            OnPointsUpdatePlayer1(pointsToAdd, EventArgs.Empty);
                        }
                        else
                        {
                            OnPointsUpdatePlayer2(pointsToAdd, EventArgs.Empty);
                        }
                    }
                    SpawnEntities(spawnEntityData.AmountToSpawn, spawnEntityData.SpawnEntity, trans, ecb, true);
                    OnEnemyHit(this, EventArgs.Empty);
                    Pooler.Instance.Spawn(particlesData.ParticlePrefabObject, trans.Value, quaternion.identity);
                    
                    if (HasComponent<UFOBigTag>(thisEntity))
                    {
                        
                        var currentLives = localUfoLivesData[thisEntity].CurrentLives - 1;
                        if (currentLives > 0)
                        {
                            localUfoLivesData[thisEntity] = new UFOLivesData() {CurrentLives = currentLives};
                        }
                        else
                        {
                            ecb.DestroyEntity(thisEntity);
                            OnBigShipDestroyed(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        if (Random.value < powerUpRandomAppearData.AppearanceChance)
                        {
                            SpawnEntities(1, powerUpRandomAppearData.RandomPowerUp, trans, ecb, false);
                        }
                        ecb.DestroyEntity(thisEntity);
                    }
                }).WithoutBurst().Run();


        Entities.WithAll<EnemyBulletTag>().WithAll<HasCollidedTag>().ForEach((Entity thisEntity) =>
        {
            ecb.DestroyEntity(thisEntity);
        }).Schedule();

        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
    
    static void SpawnEntities(int amountToSpawn, Entity entityToSpawn, Translation trans, EntityCommandBuffer ecb,
        bool spawnInRandomPosition)
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            var newEntity = ecb.Instantiate(entityToSpawn);
            float2 spawnLocation = 0;
            if (spawnInRandomPosition)
            {
                spawnLocation = Random.insideUnitCircle.normalized * 10;
            }

            ecb.SetComponent(newEntity, new Translation
            {
                Value = new float3(trans.Value.x + spawnLocation.x,
                    trans.Value.y + spawnLocation.y, -50)
            });
        }
    }
}