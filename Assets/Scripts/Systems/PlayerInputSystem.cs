using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;



public class PlayerInputSystem : SystemBase
{
    private PlayersActions _playersActions;
    public EventHandler OnPause;
    public EventHandler OnResume;
    
    protected override void OnStartRunning()
    {
        _playersActions = new PlayersActions();
        _playersActions.Enable();
        
        
    }


    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.WaitingToStart || gameState.GameState != GameStateData.State.PlayersDead)
            if (_playersActions.Player1.PauseGame.triggered)
            {
                if (gameState.GameState == GameStateData.State.Paused)
                {
                    gameState.GameState = GameStateData.State.Playing;
                    OnResume.Invoke(this,EventArgs.Empty);
                    SetSingleton(gameState);
                } else if (gameState.GameState == GameStateData.State.Playing)
                {
                    gameState.GameState = GameStateData.State.Paused;
                    SetSingleton(gameState);
                    OnPause.Invoke(this,EventArgs.Empty);
                }
            }

        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var deltaTime = Time.DeltaTime;
        if (_playersActions.Player1.Move.ReadValue<Vector2>().y > 0)
        {
            Entities.WithAll<Player1Tag>()
                .ForEach((ref MoveSpeedData speedData, in MoveAccelerationData accelerationData,
                    in Rotation rot, in MoveMaxSpeedData maxSpeedData) =>
                {
                    if (GetFloat3Magnitude(speedData.movementSpeed) < maxSpeedData.MaxSpeed)
                    {
                        speedData.movementSpeed +=
                            deltaTime * accelerationData.acceleration * math.forward(rot.Value);
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
                    rotationData.rotation =
                        CalculateRotation(localToWorld, rotationModifier, player1ActionValueX);
                }).ScheduleParallel();
        }


        if (_playersActions.Player2.Move.ReadValue<Vector2>().y > 0)
        {
            Entities.WithAll<Player2Tag>()
                .ForEach((ref MoveSpeedData speedData, in MoveAccelerationData accelerationData,
                    in Rotation rot,
                    in MoveMaxSpeedData maxSpeedData) =>
                {
                    if (GetFloat3Magnitude(speedData.movementSpeed) < maxSpeedData.MaxSpeed)
                    {
                        speedData.movementSpeed +=
                            deltaTime * accelerationData.acceleration * math.forward(rot.Value);
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
                    rotationData.rotation =
                        CalculateRotation(localToWorld, rotationModifier, player2ActionValueX);
                    ;
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
        {
            Entities.WithAll<Player2Tag>()
                .ForEach((ref BulletFireData bulletFireData) => { bulletFireData.TryFire = true; })
                .WithStructuralChanges().WithoutBurst().Run();

        }

        if (_playersActions.Player1.Hyperspace.triggered)
        {
            Entities.WithAll<Player1Tag>()
                .ForEach((ref MoveSpeedData speedData, ref Translation trans) =>
                {
                    speedData.movementSpeed = 0;
                    var hyperJumpLocation = Random.insideUnitCircle.normalized * 75;
                    trans.Value = new float3(hyperJumpLocation, -50);
                }).Run();
        }

        if (_playersActions.Player2.Hyperspace.triggered)
        {
            Entities.WithAll<Player2Tag>()
                .ForEach((ref MoveSpeedData speedData, ref Translation trans) =>
                {
                    speedData.movementSpeed = 0;
                    var hyperJumpLocation = Random.insideUnitCircle.normalized * 75;
                    trans.Value = new float3(hyperJumpLocation, -50);
                }).Run();
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

