using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PowerUpBulletData : IComponentData
{
    public Entity NewBulletEntity;
    public Entity OldBulletEntity;
    public float PowerUpTimer;
    public float PowerUpDurationSeconds;
}
  