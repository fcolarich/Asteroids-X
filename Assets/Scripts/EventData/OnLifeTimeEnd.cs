using Unity.Entities;


[GenerateAuthoringComponent]
public struct OnLifeTimeEnd : IComponentData
{
    public bool Value;
}