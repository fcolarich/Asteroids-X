using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct UFOLivesData : IComponentData
{
    public int CurrentLives;
    public float UpdateDelayTimer;
    public float UpdateDelaySeconds;
}
