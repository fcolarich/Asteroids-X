using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class PlayerInputSystem : SystemBase
{
    private PlayersActions playersActions;
    private EntityQueryDesc player1Queue; 
        
    protected override void OnStartRunning()
    {
        playersActions = new PlayersActions();
        playersActions.Enable();
    }


    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        
        if (playersActions.Player1.Move.ReadValue<Vector2>().y > 0)
        {
            Entities.WithAll<Player1Tag>().ForEach((ref MoveSpeedData speedData,in MoveAccelerationData accelerationData, in Rotation rot) =>
            {
                speedData.movementSpeed +=  deltaTime*accelerationData.acceleration * math.forward(rot.Value) ;
            }).ScheduleParallel();
        }
        var playerActionValueX = playersActions.Player1.Move.ReadValue<Vector2>().x;
        if (playerActionValueX != 0)
        {
            Entities.WithAll<Player1Tag>().ForEach((ref MoveRotationData rotationData, in LocalToWorld localToWorld, in RotationModifier rotationModifier) =>
            {
                var initialRotation = quaternion.LookRotationSafe(localToWorld.Forward, localToWorld.Up);
                var rotationAmount = quaternion.AxisAngle(new float3( 0, 0,1), rotationModifier.rotationModifier*playerActionValueX);
                var finalRotation = math.mul(math.mul(initialRotation, math.mul(math.inverse(initialRotation), rotationAmount)),
                    initialRotation);
                rotationData.rotation = finalRotation;
            }).ScheduleParallel();
        }
        
        
        if (playersActions.Player2.Move.ReadValue<Vector2>().y > 0)
        {
            Entities.WithAll<Player2Tag>().ForEach((ref MoveSpeedData speedData,in MoveAccelerationData accelerationData, in Rotation rot) =>
            {
                speedData.movementSpeed +=  deltaTime*accelerationData.acceleration * math.forward(rot.Value) ;
            }).ScheduleParallel();
        }

        var player2ActionValueX = playersActions.Player2.Move.ReadValue<Vector2>().x; 
        if (player2ActionValueX != 0)
        {
            Entities.WithAll<Player2Tag>().ForEach((ref MoveRotationData rotationData, in LocalToWorld localToWorld, in RotationModifier rotationModifier) =>
            {
                var initialRotation = quaternion.LookRotationSafe(localToWorld.Forward, localToWorld.Up);
                var rotationAmount = quaternion.AxisAngle(new float3( 0, 0,1), rotationModifier.rotationModifier*player2ActionValueX);
                var finalRotation = math.mul(math.mul(initialRotation, math.mul(math.inverse(initialRotation), rotationAmount)),
                    initialRotation);
                rotationData.rotation = finalRotation;
            }).ScheduleParallel();
        }
        
        if (playersActions.Player1.Fire.triggered)
        {
        }
        if (playersActions.Player2.Fire.triggered)
        {
        }
    }
}
