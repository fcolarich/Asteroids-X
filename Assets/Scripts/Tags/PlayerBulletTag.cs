using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerBulletTag : IComponentData
{
    public bool IsPiercing;
}
