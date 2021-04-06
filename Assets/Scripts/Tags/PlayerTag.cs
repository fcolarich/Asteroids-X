using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerTag : IComponentData
{
    public bool IsPlayer1;
    public bool IsPlayer2;
}