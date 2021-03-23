using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameWavesControlSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem _beginSimulationEcbSystem;
    private bool _newWave = true;
    private bool _resetGame = true;
    private int _amountToSpawn;
    public EventHandler OnEnemyShipCreated;
    public EventHandler OnEnemyBigShipCreated;



    protected override void OnCreate()
    {
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();

        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();

        if (gameState.GameState == GameStateData.State.PlayersDead)
        {
            if (_resetGame)
            {
                Entities.WithNone<PowerUpPrefab>().WithAny<MoveSpeedData>().WithAny<PowerUpTag>().ForEach((Entity thisEntity) => 
                        { ecb.DestroyEntity(thisEntity); })
                    .WithoutBurst().Run();
                _newWave = true;
                Entities.ForEach((ref WaveManagerData waveManagerData) => { waveManagerData.CurrentWave = 0; })
                    .ScheduleParallel();
                _resetGame = false;
            }
        }
        
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        int tempAmountToSpawn = 0;
        var deltaTime = Time.DeltaTime;


        if (_newWave)
        {
            _newWave = false;
            _resetGame = true;
            Entities.ForEach((ref WaveManagerData waveManagerData) =>
            {
                waveManagerData.CurrentWave += 1;
                tempAmountToSpawn = waveManagerData.StartingAmountToSpawn +
                                    (waveManagerData.CurrentWave * waveManagerData.IncrementPerWave);

                for (int i = 0; i < tempAmountToSpawn; i++)
                {
                    var spawnLocation = Random.insideUnitCircle.normalized * 300;
                    var newEntity = ecb.Instantiate(waveManagerData.AsteroidPrefab);
                    ecb.SetComponent(newEntity,
                        new Translation() {Value = new float3(spawnLocation.x+100, spawnLocation.y, -50)});
                }

                var modWaves = waveManagerData.CurrentWave % waveManagerData.BigUFOWaveIntervals;
                if (modWaves == 0)
                {
                    var spawnLocation = Random.insideUnitCircle.normalized * 150;
                    var newEntity = ecb.Instantiate(waveManagerData.BigUFOPrefab);
                    ecb.SetComponent(newEntity,
                        new Translation() {Value = new float3(spawnLocation.x, spawnLocation.y, -50)});
                    OnEnemyBigShipCreated(this,EventArgs.Empty);
                }

            }).WithoutBurst().Run();
            _amountToSpawn = tempAmountToSpawn;
        }
        
        
        Entities.ForEach((ref WaveManagerData waveManagerData) =>
        {
            var destroyedAsteroidsAmount = GetEntityQuery(ComponentType.ReadOnly<SmallAsteroidDestroyedTag>())
                .CalculateEntityCount();
            if (destroyedAsteroidsAmount > _amountToSpawn - 4 && waveManagerData.SpawnTimer <= 0)
            {
                if (Random.value > 0.5)
                {
                    var spawnLocation = Random.insideUnitCircle.normalized * 150;
                    var newEntity = ecb.Instantiate(waveManagerData.SmallUFOPrefab);
                    ecb.SetComponent(newEntity,
                        new Translation() {Value = new float3(spawnLocation.x + 100, spawnLocation.y, -50)});
                    waveManagerData.SpawnTimer = waveManagerData.TimeBetweenTrySpawnsSeconds;
                    OnEnemyShipCreated(this, EventArgs.Empty);
                }

                if (Random.value > 0.7)
                {
                    var spawnLocation = Random.insideUnitCircle.normalized * 40;
                    var newEntity = ecb.Instantiate(waveManagerData.MediumUFOPrefab);
                    ecb.SetComponent(newEntity,
                        new Translation() {Value = new float3(spawnLocation.x + 150, spawnLocation.y, -50)});
                    waveManagerData.SpawnTimer = waveManagerData.TimeBetweenTrySpawnsSeconds;
                    OnEnemyShipCreated(this, EventArgs.Empty);
                }
            }
            else
            {
                waveManagerData.SpawnTimer -= deltaTime;
            }
        }).WithoutBurst().Run();


        var destroyedAsteroidsAmount = GetEntityQuery(ComponentType.ReadOnly<SmallAsteroidDestroyedTag>())
            .CalculateEntityCount();
        if (destroyedAsteroidsAmount >= _amountToSpawn * 4)
        {
            _newWave = true;
            Entities.WithAll<SmallAsteroidDestroyedTag>().ForEach((Entity thisEntity) =>
                {
                    ecb.DestroyEntity(thisEntity);
                })
                .Run();
        }

        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }

}
