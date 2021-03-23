using Unity.Entities;

[GenerateAuthoringComponent]
public struct CollisionControlData : IComponentData
{
    //public bool HasCollided;
    public Entity AffectedTarget;
}