using Unity.Entities;


[GenerateAuthoringComponent]
public struct PowerUpRandomAppearData : IComponentData
{
    public Entity RandomPowerUp;
    public float AppearanceChance;
}