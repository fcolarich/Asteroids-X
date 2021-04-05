using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class GameWavesControlSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem _beginSimulationEcbSystem;
    private bool _newWave = true;
    private bool _resetGame = true;
    private bool _startNewWave = false;


    protected override void OnCreate()
    {
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();

        if (gameState.GameState == GameStateData.State.PlayersDead)
        {
            if (_resetGame)
            {
                Entities.WithNone<PowerUpPrefab>().WithAny<MoveSpeedData>().WithAny<PowerUpTag>().ForEach((int entityInQueryIndex, Entity thisEntity) => 
                        { ecb.DestroyEntity(entityInQueryIndex,thisEntity); })
                    .ScheduleParallel();
                _newWave = true;
                Entities.ForEach((ref WaveManagerData waveManagerData) => { waveManagerData.CurrentWave = 0; })
                    .ScheduleParallel();
                _resetGame = false;
                _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
            }
            
        }
        
        if (gameState.GameState != GameStateData.State.Playing) return;
        

        var deltaTime = Time.DeltaTime;
        var elapsedTime = Time.ElapsedTime;


        if (_newWave)
        {
            _newWave = false;
            _resetGame = true;
            Entities.ForEach((int entityInQueryIndex, ref WaveManagerData waveManagerData) =>
            {
                waveManagerData.CurrentWave += 1;
                waveManagerData.CurrentAmountToSpawn = waveManagerData.StartingAmountToSpawn +
                                                       (waveManagerData.CurrentWave * waveManagerData.IncrementPerWave);

                var tempAmountToSpawn = waveManagerData.CurrentAmountToSpawn;
                for (int i = 0; i < tempAmountToSpawn; i++)
                {
                    var spawnLocation = RandomPointInCircle(elapsedTime*(i+1)) * 300;
                    var newEntity = ecb.Instantiate(entityInQueryIndex,waveManagerData.AsteroidPrefab);
                    ecb.SetComponent(entityInQueryIndex,newEntity,
                        new Translation() {Value = new float3(spawnLocation.x+100, spawnLocation.y, -50)});
                    ecb.SetComponent(entityInQueryIndex,newEntity, new ToInitialize() {Value = true});
                }

                var modWaves = waveManagerData.CurrentWave % waveManagerData.BigUFOWaveIntervals;
                
                if (modWaves == 0)
                {
                    var spawnLocation = RandomPointInCircle(elapsedTime*waveManagerData.CurrentWave) * 150;
                    var newEntity = ecb.Instantiate(entityInQueryIndex,waveManagerData.BigUFOPrefab);
                    ecb.SetComponent(entityInQueryIndex,newEntity,
                        new Translation() {Value = new float3(spawnLocation.x, spawnLocation.y, -50)});
                    ecb.SetComponent(entityInQueryIndex,newEntity, new OnEnemyBigShipCreated {Value = true});
                }
            }).ScheduleParallel();
        }

        Entities.ForEach((int entityInQueryIndex,ref WaveManagerTimerData waveManagerTimerData,in WaveManagerData waveManagerData) =>
        {
            if (waveManagerTimerData.SpawnTimer <= 0)
            {
                if (Unity.Mathematics.Random.CreateFromIndex(Convert.ToUInt32((1+entityInQueryIndex)*elapsedTime)).NextFloat() > 0.5)
                {
                    var spawnLocation = RandomPointInCircle((elapsedTime*(1+waveManagerData.CurrentWave))) * 150;
                    var newEntity = ecb.Instantiate(entityInQueryIndex,waveManagerData.SmallUFOPrefab);
                    ecb.SetComponent(entityInQueryIndex,newEntity,
                        new Translation() {Value = new float3(spawnLocation.x + 100, spawnLocation.y, -50)});
                    waveManagerTimerData.SpawnTimer = waveManagerData.TimeBetweenTrySpawnsSeconds;
                    ecb.SetComponent(entityInQueryIndex,newEntity, new OnEnemyShipCreated() {Value = true});
                }

                if (Unity.Mathematics.Random.CreateFromIndex(Convert.ToUInt32(elapsedTime)).NextFloat() > 0.7)
                {
                    var spawnLocation = RandomPointInCircle(elapsedTime*(2+waveManagerData.CurrentWave)) * 80;
                    var newEntity = ecb.Instantiate(entityInQueryIndex,waveManagerData.MediumUFOPrefab);
                    ecb.SetComponent(entityInQueryIndex,newEntity,
                        new Translation() {Value = new float3(spawnLocation.x + 500, spawnLocation.y, -50)});
                    waveManagerTimerData.SpawnTimer = waveManagerData.TimeBetweenTrySpawnsSeconds;
                    ecb.SetComponent(entityInQueryIndex,newEntity, new OnEnemyShipCreated() {Value = true});
                }
            }
            else
            {
                waveManagerTimerData.SpawnTimer -= deltaTime;
            }
        }).ScheduleParallel();

        var destroyedAsteroidsAmount = GetEntityQuery(ComponentType.ReadOnly<SmallAsteroidDestroyedTag>())
            .CalculateEntityCount();

        Entities.ForEach((in WaveManagerData waveManagerData) =>
        {
            if (destroyedAsteroidsAmount >= waveManagerData.CurrentAmountToSpawn * 4)
            {
                _startNewWave = true;
            };
        }).WithoutBurst().Run();
        
        if (_startNewWave)
        {
            _newWave = true;
            Entities.WithAll<SmallAsteroidDestroyedTag>().ForEach((int entityInQueryIndex, Entity thisEntity) =>
                {
                    ecb.DestroyEntity(entityInQueryIndex,thisEntity);
                })
                .ScheduleParallel();
            _startNewWave = false;
        }

        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }

    static float2 RandomPointInCircle(in double someIndex)
    {
        var index = Unity.Mathematics.Random.CreateFromIndex(Convert.ToUInt32(someIndex)).NextUInt();
        var randomPointInCircleX = math.cos(Unity.Mathematics.Random.CreateFromIndex(index).NextFloat(360));
        var randomPointInCircleY = math.sin(Unity.Mathematics.Random.CreateFromIndex(index).NextFloat(360));
        return new float2(randomPointInCircleX, randomPointInCircleY);
    }
}
