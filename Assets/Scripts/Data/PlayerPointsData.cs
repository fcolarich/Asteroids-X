using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerPointsData : IComponentData
{
  public int points;
}
