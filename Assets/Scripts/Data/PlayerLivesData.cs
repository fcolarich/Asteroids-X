using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerLivesData : IComponentData
{
  public int CurrentLives;
  public float UpdateDelayTimer;
  public float UpdateDelaySeconds;
  public float3 OriginPosition;
}
