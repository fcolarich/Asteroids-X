using Unity.Entities;


[GenerateAuthoringComponent]
public struct OnLivesUpdated : IComponentData
{
    public bool value;
}