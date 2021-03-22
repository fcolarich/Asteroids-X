using Unity.Entities;

[GenerateAuthoringComponent]
public struct GameStateData : IComponentData
{
    public enum State
    {
        WaitingToStart,
        Playing,
        Paused,
        PlayersDead
    }
    public State GameState;
}