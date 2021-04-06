using Unity.Entities;
using Unity.Mathematics;

public class PlayerFrictionSpeedSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var deltaTime = Time.DeltaTime;

        Entities.WithAll<PlayerTag>().ForEach(
            (ref MoveSpeedData moveSpeedData, in MoveSpeedModifierData speedModifierData) =>
            {
                var movementSpeed = moveSpeedData.movementSpeed;
                var speedModifier = speedModifierData.SpeedModifier;

                if (movementSpeed.x > 0)
                {
                    moveSpeedData.movementSpeed.x -= speedModifier * deltaTime;
                }
                else if (movementSpeed.x < 0)
                {
                    moveSpeedData.movementSpeed.x += speedModifier * deltaTime;
                }

                if (movementSpeed.y > 0)
                {
                    moveSpeedData.movementSpeed.y -= speedModifier * deltaTime;
                }
                else if (movementSpeed.y < 0)
                {
                    moveSpeedData.movementSpeed.y += speedModifier * deltaTime;
                }
            }).ScheduleParallel();
    }
}
