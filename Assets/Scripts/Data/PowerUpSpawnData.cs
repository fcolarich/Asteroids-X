using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct PowerUpSpawnData : IComponentData
{
    public float PowerUpTimer;
    public float PowerUpDurationSeconds;
}