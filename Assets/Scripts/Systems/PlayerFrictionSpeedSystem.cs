using Unity.Entities;
using Unity.Mathematics;

public class PlayerFrictionSpeedSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        
        Entities.WithAny<Player1Tag>().WithAny<Player2Tag>().ForEach((ref MoveSpeedData moveSpeedData, in MoveSpeedModifierData speedModifierData) =>
        {
            var movementSpeed = moveSpeedData.movementSpeed;
            var speedModifier = speedModifierData.SpeedModifier;

            if (movementSpeed.x > 0)
            {
                moveSpeedData.movementSpeed.x -= speedModifier*deltaTime;
            } else if (movementSpeed.x < 0)
            {
                moveSpeedData.movementSpeed.x += speedModifier*deltaTime;
            }
            
            if (movementSpeed.y > 0)
            {
                moveSpeedData.movementSpeed.y -= speedModifier*deltaTime;
            } else if (movementSpeed.y < 0)
            {
                moveSpeedData.movementSpeed.y += speedModifier*deltaTime;
            }
        }).ScheduleParallel();

    }
}
