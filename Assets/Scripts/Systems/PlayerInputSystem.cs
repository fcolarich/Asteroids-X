using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;



public class PlayerInputSystem : SystemBase
{
    private PlayersActions _playersActions;
    public EventHandler OnPause;
    public EventHandler OnResume;
    public EventHandler OnStart;
    public EventHandler OnRestart;
    public EventHandler OnPlayer2Join;
    public EventHandler OnSkipVideo;
    public EventHandler OnOptions;

    private bool _player2Spawned = false;
    
    protected override void OnStartRunning()
    {
        _playersActions = new PlayersActions();
        _playersActions.Enable();
    }
    

    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        switch (gameState.GameState)
        {
            case GameStateData.State.InOptionsMenu:
            {
                if (_playersActions.Player1.PauseGame.triggered)
                {
                    OnStart(this,EventArgs.Empty);
                    gameState.GameState = GameStateData.State.WaitingToStart;
                    SetSingleton(gameState);
                    OnOptions.Invoke(false,EventArgs.Empty);
                }
                break;
            }
            case GameStateData.State.WaitingToStart:
            {
                if (_playersActions.Player1.StartGame.triggered)
                {
                    OnStart(this,EventArgs.Empty);
                    gameState.GameState = GameStateData.State.Playing;
                    SetSingleton(gameState);
                    Entities.ForEach((in Player1SpawnData player1SpawnData) =>
                    { 
                        EntityManager.Instantiate(player1SpawnData.Player1Prefab);
                    }).WithoutBurst().WithStructuralChanges().Run();
                }
                if (_playersActions.Player1.PauseGame.triggered)
                {
                    OnStart(this,EventArgs.Empty);
                    gameState.GameState = GameStateData.State.InOptionsMenu;
                    SetSingleton(gameState);
                    OnOptions.Invoke(true,EventArgs.Empty);
                }
                if (_playersActions.Player2.Fire.triggered && !_player2Spawned)
                {
                    OnPlayer2Join(this,EventArgs.Empty);
                    _player2Spawned = true;
                    Entities.ForEach((in Player2SpawnData player2SpawnData) =>
                    {
                        var player2 = EntityManager.Instantiate(player2SpawnData.Player2Prefab);
                    }).WithoutBurst().WithStructuralChanges().Run();
                }
                break;
            }
            case GameStateData.State.PlayersDead:
            {
                if (_playersActions.Player1.StartGame.triggered)
                {
                    Entities.ForEach((in Player1SpawnData player1SpawnData) =>
                    { 
                        EntityManager.Instantiate(player1SpawnData.Player1Prefab);
                    }).WithoutBurst().WithStructuralChanges().Run();
                    gameState.GameState = GameStateData.State.Playing;
                    SetSingleton(gameState);
                    _player2Spawned = false;
                    OnRestart(this, EventArgs.Empty);
                }
                break;
            }
        }
        
            if (_playersActions.Player1.PauseGame.triggered)
            {
                if (gameState.GameState == GameStateData.State.Paused)
                {
                    gameState.GameState = GameStateData.State.Playing;
                    OnResume(this,EventArgs.Empty);
                    SetSingleton(gameState);
                } else if (gameState.GameState == GameStateData.State.Playing)
                {
                    gameState.GameState = GameStateData.State.Paused;
                    SetSingleton(gameState);
                    OnPause(this,EventArgs.Empty);
                }
            }

        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var deltaTime = Time.DeltaTime;
        
        if (_playersActions.Player1.Move.ReadValue<Vector2>().y > 0)
        {
            Entities.ForEach((ref MoveSpeedData speedData, in MoveAccelerationData accelerationData,
                    in Rotation rot, in PlayerTag playerTag, in MoveMaxSpeedData maxSpeedData) =>
                {
                    if (playerTag.IsPlayer1)
                    {
                        if (GetFloat3Magnitude(speedData.movementSpeed) < maxSpeedData.MaxSpeed)
                        {
                            speedData.movementSpeed +=
                                deltaTime * accelerationData.acceleration * math.forward(rot.Value);
                        }
                    }
                    
                }).ScheduleParallel();
        }

        var player1ActionValueX = _playersActions.Player1.Move.ReadValue<Vector2>().x;
        if (player1ActionValueX != 0)
        {
            Entities.ForEach((ref MoveRotationData rotationData, in LocalToWorld localToWorld, in PlayerTag playerTag,
                    in MoveRotationModifierData rotationModifier) =>
                {
                    if (playerTag.IsPlayer1)
                    {
                        rotationData.rotation =
                            CalculateRotation(localToWorld, rotationModifier, player1ActionValueX);
                    }
                }).ScheduleParallel();
        }


        if (_playersActions.Player2.Move.ReadValue<Vector2>().y > 0)
        {
            Entities.ForEach((ref MoveSpeedData speedData, in MoveAccelerationData accelerationData, in PlayerTag playerTag,
                    in Rotation rot, in MoveMaxSpeedData maxSpeedData) =>
                {
                    if (playerTag.IsPlayer2)
                    {
                        if (GetFloat3Magnitude(speedData.movementSpeed) < maxSpeedData.MaxSpeed)
                        {
                            speedData.movementSpeed +=
                                deltaTime * accelerationData.acceleration * math.forward(rot.Value);
                        }
                    }
                }).ScheduleParallel();
        }

        var player2ActionValueX = _playersActions.Player2.Move.ReadValue<Vector2>().x;
        if (player2ActionValueX != 0)
        {
            Entities.ForEach((ref MoveRotationData rotationData, in LocalToWorld localToWorld,in PlayerTag playerTag,
                    in MoveRotationModifierData rotationModifier) =>
                {
                    if (playerTag.IsPlayer2)
                    {
                        rotationData.rotation =
                            CalculateRotation(localToWorld, rotationModifier, player2ActionValueX);
                    }
                }).ScheduleParallel();
        }

        if (_playersActions.Player1.Fire.triggered)
        {
            Entities.ForEach((ref BulletFireData bulletFireData, in PlayerTag playerTag) =>
            {
                if (playerTag.IsPlayer1)
                {
                    bulletFireData.TryFire = true;
                }
            }).ScheduleParallel();
        }

        if (_playersActions.Player2.Fire.triggered)
        {
            Entities
                .ForEach((ref BulletFireData bulletFireData, in PlayerTag playerTag) =>
                {
                    if (playerTag.IsPlayer2)
                    {
                        bulletFireData.TryFire = true;
                    }
                }).ScheduleParallel();
            
            var playerAmount = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>()).CalculateEntityCount();
            if (!_player2Spawned)
            {
                _player2Spawned = true;
                OnPlayer2Join(this,EventArgs.Empty);
                Entities.ForEach((in Player2SpawnData player2SpawnData) =>
                { 
                    EntityManager.Instantiate(player2SpawnData.Player2Prefab);
                }).WithStructuralChanges().WithoutBurst().Run();
            }
        }

        if (_playersActions.Player1.Hyperspace.triggered)
        {
            Entities
                .ForEach((GameObjectParticleData gameObjectParticleData, ref MoveSpeedData speedData, ref Translation trans, in PlayerTag playerTag) =>
                {
                    if (playerTag.IsPlayer1)
                    {
                        speedData.movementSpeed = 0;
                        var hyperJumpLocation = Random.insideUnitCircle.normalized * 90;
                        Pooler.Instance.Spawn(gameObjectParticleData.ParticleGameObject,
                            new Vector3(trans.Value.x, trans.Value.y, -45), Quaternion.identity);
                        trans.Value = new float3(hyperJumpLocation, -50);
                        Pooler.Instance.Spawn(gameObjectParticleData.ParticleGameObject,
                            new Vector3(trans.Value.x, trans.Value.y, -45), Quaternion.identity);
                    }
                }).WithoutBurst().Run();
        }

        if (_playersActions.Player2.Hyperspace.triggered)
        {
            Entities.ForEach((GameObjectParticleData gameObjectParticleData, ref MoveSpeedData speedData, 
                ref Translation trans, in PlayerTag playerTag) =>
                {
                    if (playerTag.IsPlayer2)
                    {
                        speedData.movementSpeed = 0;
                        var hyperJumpLocation = Random.insideUnitCircle.normalized * 90;
                        Pooler.Instance.Spawn(gameObjectParticleData.ParticleGameObject,
                            new Vector3(trans.Value.x, trans.Value.y, -45), Quaternion.identity);
                        trans.Value = new float3(hyperJumpLocation, -50);
                        Pooler.Instance.Spawn(gameObjectParticleData.ParticleGameObject,
                            new Vector3(trans.Value.x, trans.Value.y, -45), Quaternion.identity);
                    }
                }).WithoutBurst().Run();
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

