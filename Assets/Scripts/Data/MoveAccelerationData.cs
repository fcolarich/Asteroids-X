using Unity.Entities;

[GenerateAuthoringComponent]
public struct MoveAccelerationData : IComponentData
{
    public float acceleration;
}