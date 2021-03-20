using Unity.Entities;
using UnityEngine;


[GenerateAuthoringComponent]

public struct SpawnEntityData : IComponentData
{
  public int AmountToSpawn;
  public Entity SpawnEntity;
}
