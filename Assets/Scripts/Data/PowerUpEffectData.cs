using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PowerUpEffectData : IComponentData
{
    public float PowerUpTimer;
    public float PowerUpDurationSeconds;
}