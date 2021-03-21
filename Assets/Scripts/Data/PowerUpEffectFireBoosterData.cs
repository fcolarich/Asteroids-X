using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PowerUpEffectFireBoosterData : IComponentData
{
    public float FireRateSecondsReduction;
}