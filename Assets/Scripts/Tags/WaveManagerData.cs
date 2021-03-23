using Unity.Entities;


[GenerateAuthoringComponent]

public struct WaveManagerData : IComponentData
{
    public int StartingAmountToSpawn;
    public int IncrementPerWave;
    public int CurrentWave;
    public Entity AsteroidPrefab;
    public Entity SmallUFOPrefab;
    public Entity MediumUFOPrefab;
    public Entity BigUFOPrefab;
    public float SpawnTimer;
    public float TimeBetweenTrySpawnsSeconds;
    public int BigUFOWaveIntervals;
}
