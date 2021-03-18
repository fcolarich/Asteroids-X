using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct MoveSpeedData : IComponentData
{
    public float3 movementSpeed;
}
