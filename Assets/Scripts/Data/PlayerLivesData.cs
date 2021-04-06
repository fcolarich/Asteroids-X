using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerLivesData : IComponentData
{
  public int CurrentLives;
  public bool CanTakeDamage;
  public float3 OriginPosition;
  public int EarnedLives;
}
