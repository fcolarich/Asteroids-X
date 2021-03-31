using Unity.Entities;


[GenerateAuthoringComponent]
public struct OnDestroyed : IComponentData
{
    public bool Value;
}