using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct PowerUpData : IComponentData
{
    public float PowerUpTimer;
    public float PowerUpDurationSeconds;
}