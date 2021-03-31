using Unity.Entities;


[GenerateAuthoringComponent]
public struct OnEnemyHit : IComponentData
{
    public bool Value;
}