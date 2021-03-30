using Unity.Entities;


[GenerateAuthoringComponent]
public struct OnCollision : IComponentData
{
    public bool Value;
}