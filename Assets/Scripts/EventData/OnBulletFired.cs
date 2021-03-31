using Unity.Entities;


[GenerateAuthoringComponent]
public struct OnBulletFired : IComponentData
{
    public bool Value;
}