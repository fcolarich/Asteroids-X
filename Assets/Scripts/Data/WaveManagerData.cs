using Unity.Entities;


[GenerateAuthoringComponent]

public struct WaveManagerData : IComponentData
{
    public int StartingAsteroidAmountToSpawn;
    public int AditionalAsteroidsPerWave;
    public int CurrentWave;
    public int CurrentAsteroidAmountToSpawn;
    public Entity AsteroidPrefab;
    public Entity SmallUFOPrefab;
    public Entity MediumUFOPrefab;
    public Entity BigUFOPrefab;
    public float UFOSpawnTimer;
    public float TimeBetweenTrySpawnsSeconds;
    public int BigUFOWaveIntervals;
}
