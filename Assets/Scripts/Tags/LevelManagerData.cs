using Unity.Entities;


[GenerateAuthoringComponent]

public struct LevelManagerData : IComponentData
{
    public int StartingAmountToSpawn;
    public int IncrementPerWave;
    public int CurrentWave;
    public Entity SpawnEntity;
    public bool NewWave;
}