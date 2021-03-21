using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct UFOGeneralData : IComponentData
{
 public Entity targetEntity;
 public float3 targetDirection;
}
