using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Player2SpawnData : IComponentData
{
    public Entity Player2Prefab;
    public float3 Player2Position;
}