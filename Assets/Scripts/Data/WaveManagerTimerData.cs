using Unity.Entities;


[GenerateAuthoringComponent]

public struct WaveManagerTimerData : IComponentData
{
    public float SpawnTimer;
    public float TimeBetweenTrySpawnsSeconds;
}
