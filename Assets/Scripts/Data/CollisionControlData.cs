using Unity.Entities;

[GenerateAuthoringComponent]
public struct CollisionControlData : IComponentData
{
    public Entity AffectedTarget;
}