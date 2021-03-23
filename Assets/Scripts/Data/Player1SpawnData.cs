using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Player1SpawnData : IComponentData
{
    public Entity Player1Prefab;
}