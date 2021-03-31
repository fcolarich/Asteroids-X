using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

public class EnemyCollisionSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;

    protected override void OnCreate()
    {
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;


        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
        var elapsedTime = Time.ElapsedTime;


        Entities.WithChangeFilter<OnCollision>().WithAny<AsteroidsTag>()
            .WithAny<UFOSmallTag>().WithAny<UFOMediumTag>().WithAny<UFOBigTag>().ForEach(
                (int entityInQueryIndex, Entity thisEntity, ref OnCollision onCollision, ref OnEnemyHit onEnemyHit, in Translation trans,
                    in SpawnEntityData spawnEntityData,
                    in PowerUpRandomAppearData powerUpRandomAppearData) =>
                {
                    if (onCollision.Value)
                    {
                        onCollision.Value = false;
                        SpawnEntities(entityInQueryIndex, spawnEntityData.AmountToSpawn, spawnEntityData.SpawnEntity, trans, ecb, true);

                        var index = Random.CreateFromIndex(Convert.ToUInt32(entityInQueryIndex*elapsedTime)).NextUInt();

                        if (Random.CreateFromIndex(index).NextFloat(1) < powerUpRandomAppearData.AppearanceChance)
                        {
                            var newEntity = ecb.Instantiate(entityInQueryIndex,powerUpRandomAppearData.RandomPowerUp);
                            ecb.SetComponent(entityInQueryIndex,newEntity, new Translation
                            { Value = new float3(trans.Value.x, trans.Value.y, -50)});
                        }

                        onEnemyHit.Value = true;
                    }
                }).ScheduleParallel();


        Entities.WithChangeFilter<OnCollision>().WithAny<EnemyBulletTag>().ForEach((int entityInQueryIndex, Entity thisEntity, ref OnDestroyed onDestroyed, in OnCollision onCollision) =>
        {
            if (onCollision.Value)
            {
                onDestroyed.Value = true;
            }
        }).ScheduleParallel();

        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
    
    static void SpawnEntities(int entityInQueryIndex, int amountToSpawn, Entity entityToSpawn, Translation trans, EntityCommandBuffer.ParallelWriter ecb,
        bool spawnInRandomPosition)
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            float2 spawnLocationModifier = 0;
            
            if (spawnInRandomPosition)
            {                        
                var index = Random.CreateFromIndex(Convert.ToUInt32(entityInQueryIndex)).NextUInt();
                var randomPointInCircleX = math.cos(Random.CreateFromIndex(index).NextFloat());
                var randomPointInCircleY = math.sin(Random.CreateFromIndex(index).NextFloat());
                spawnLocationModifier = new float2(randomPointInCircleX, randomPointInCircleY) * 10;
            }
            var newEntity = ecb.Instantiate(entityInQueryIndex,entityToSpawn);
            ecb.SetComponent(entityInQueryIndex,newEntity, new Translation
            {
                Value = new float3(trans.Value.x + spawnLocationModifier.x,
                    trans.Value.y + spawnLocationModifier.y, -50)
            });
            ecb.SetComponent(entityInQueryIndex, newEntity, new ToInitialize {Value = true});

        }
    }
}