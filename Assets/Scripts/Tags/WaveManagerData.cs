using Unity.Entities;


[GenerateAuthoringComponent]

public struct WaveManagerData : IComponentData
{
    public int StartingAmountToSpawn;
    public int IncrementPerWave;
    public int CurrentWave;
    public Entity SpawnEntity;
    public bool NewWave;
}