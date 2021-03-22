using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class PlayerCollisionSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    public EventHandler OnLivesUpdatePlayer1;
    public EventHandler OnLivesUpdatePlayer2;
    public EventHandler OnPlayersDestroyed;
    public EventHandler OnPlayerShot;



    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        var deltaTime = Time.DeltaTime;

        Entities.ForEach((Entity thisEntity, ref PlayerLivesData playerLivesData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref MoveSpeedData moveSpeedData) =>
        {
            if (collisionControlData.HasCollided)
            {
                collisionControlData.HasCollided = false;
                if (playerLivesData.UpdateDelayTimer <= 0)
                {
                    playerLivesData.CurrentLives -= 1;
                    if (HasComponent<Player1Tag>(thisEntity))
                    {
                        OnLivesUpdatePlayer1(playerLivesData.CurrentLives, EventArgs.Empty);
                    }
                    else
                    {
                        OnLivesUpdatePlayer2(playerLivesData.CurrentLives, EventArgs.Empty);
                    }
                    OnPlayerShot(this,EventArgs.Empty);
                    
                    //TO INSTANTIATE EXPLOSION FX WE NEED THE LOCATION, WILL HAVE TO EDIT EVENT TO SEND LOCATION
                    
                    if (playerLivesData.CurrentLives < 1)
                    {
                        ecb.DestroyEntity(thisEntity);
                    }
                    else
                    {
                        playerLivesData.UpdateDelayTimer = playerLivesData.UpdateDelaySeconds;
                        trans.Value = playerLivesData.OriginPosition;
                        moveSpeedData.movementSpeed = 0;
                    }
                }
            }

            playerLivesData.UpdateDelayTimer -= deltaTime;

        }).WithoutBurst().Run();
        
        Entities.WithAll<PlayerBulletTag>().ForEach((Entity thisEntity,
            in CollisionControlData collisionControlData) =>
        {
            if (collisionControlData.HasCollided)
            {
                ecb.DestroyEntity(thisEntity);
            }
        }).Schedule();
        
        
        var playerCount = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>()).CalculateEntityCount();
        if (playerCount == 0)
        {
            gameState.GameState = GameStateData.State.PlayersDead;
            SetSingleton(gameState);
            OnPlayersDestroyed(this, EventArgs.Empty);
        }
        
        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}
