using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


public class PlayerInputSystem : SystemBase
{
    private PlayersActions _playersActions;

    protected override void OnStartRunning()
    {
        _playersActions = new PlayersActions();
        _playersActions.Enable();
    }


    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;

        if (_playersActions.Player1.Move.ReadValue<Vector2>().y > 0)
        {
            Entities.WithAll<Player1Tag>()
                .ForEach((ref MoveSpeedData speedData, in MoveAccelerationData accelerationData, in Rotation rot, in MoveMaxSpeedData maxSpeedData) =>
                {
                    if (GetFloat3Magnitude(speedData.movementSpeed) < maxSpeedData.MaxSpeed)
                    {
                       speedData.movementSpeed += deltaTime * accelerationData.acceleration * math.forward(rot.Value);
                    }
                }).ScheduleParallel();
        }

        var player1ActionValueX = _playersActions.Player1.Move.ReadValue<Vector2>().x;
        if (player1ActionValueX != 0)
        {
            Entities.WithAll<Player1Tag>().ForEach(
                (ref MoveRotationData rotationData, in LocalToWorld localToWorld,
                    in MoveRotationModifierData rotationModifier) =>
                {
                    rotationData.rotation = CalculateRotation(localToWorld, rotationModifier, player1ActionValueX);
                }).ScheduleParallel();
        }


        if (_playersActions.Player2.Move.ReadValue<Vector2>().y > 0)
        {
            Entities.WithAll<Player2Tag>()
                .ForEach((ref MoveSpeedData speedData, in MoveAccelerationData accelerationData, in Rotation rot,
                    in MoveMaxSpeedData maxSpeedData) =>
                {
                    if (GetFloat3Magnitude(speedData.movementSpeed) < maxSpeedData.MaxSpeed)
                    {
                        speedData.movementSpeed += deltaTime * accelerationData.acceleration * math.forward(rot.Value);
                    }
                }).ScheduleParallel();
        }

        var player2ActionValueX = _playersActions.Player2.Move.ReadValue<Vector2>().x;
        if (player2ActionValueX != 0)
        {
            Entities.WithAll<Player2Tag>().ForEach(
                (ref MoveRotationData rotationData, in LocalToWorld localToWorld,
                    in MoveRotationModifierData rotationModifier) =>
                {
                    rotationData.rotation =  CalculateRotation(localToWorld, rotationModifier, player2ActionValueX);;
                }).ScheduleParallel();
        }

        if (_playersActions.Player1.Fire.triggered)
        {
            Entities.WithAll<Player1Tag>().ForEach((ref BulletFireData bulletFireData) =>
            {
                bulletFireData.TryFire = true;
            }).WithStructuralChanges().WithoutBurst().Run();
        }

        if (_playersActions.Player2.Fire.triggered)
        {Entities.WithAll<Player2Tag>().ForEach((ref BulletFireData bulletFireData) =>
            {
                bulletFireData.TryFire = true;
            }).WithStructuralChanges().WithoutBurst().Run();
            
        }
    }


    static float GetFloat3Magnitude(float3 vector)
    {
        return math.sqrt(math.pow(vector.x, 2) + math.pow(vector.y, 2)+ math.pow(vector.z, 2));
    }
    
    
    static quaternion CalculateRotation(in LocalToWorld localToWorld,
        in MoveRotationModifierData moveRotationModifier, in float playerActionValueX)
    {
        var initialRotation = quaternion.LookRotationSafe(localToWorld.Forward, localToWorld.Up);
        var rotationAmount = quaternion.AxisAngle(new float3(0, 0, 1),
            moveRotationModifier.RotationModifier * playerActionValueX);
        var finalRotation = math.mul(
            math.mul(initialRotation, math.mul(math.inverse(initialRotation), rotationAmount)),
            initialRotation);
        return finalRotation;
    }
    
    
}

