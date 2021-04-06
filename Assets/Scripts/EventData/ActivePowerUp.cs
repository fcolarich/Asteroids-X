using Unity.Entities;


[GenerateAuthoringComponent]
public struct ActivePowerUp : IComponentData
{
    public bool Value;
}