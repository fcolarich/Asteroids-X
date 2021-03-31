using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct UFOGeneralData : IComponentData
{
 public Entity TargetEntity;
 public float3 TargetDirection;
}
