using Unity.Entities;


[GenerateAuthoringComponent]
public struct OnCollisionRegistered : IComponentData
{
    public bool Value;
}