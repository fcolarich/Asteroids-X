using Unity.Entities;


[GenerateAuthoringComponent]
public struct PowerUpPrefab : IComponentData
{
    public float AppearanceTimer;
    public float AppearanceTimeSeconds;
}