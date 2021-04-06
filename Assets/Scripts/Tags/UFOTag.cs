using Unity.Entities;


[GenerateAuthoringComponent]
public struct UFOTag : IComponentData
{
    public bool IsMediumUFO;
    public bool IsSmallUFO;
    public bool IsBigUFO;
}
