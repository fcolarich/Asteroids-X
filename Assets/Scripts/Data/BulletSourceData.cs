using Unity.Entities;


[GenerateAuthoringComponent]
public struct BulletSourceData : IComponentData
{
    public Entity Source;
}