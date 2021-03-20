using Unity.Entities;


[GenerateAuthoringComponent]
public struct MoveSpeedModifierData : IComponentData
{
    public float SpeedModifier;
}
