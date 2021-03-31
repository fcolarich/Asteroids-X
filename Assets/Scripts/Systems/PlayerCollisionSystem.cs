using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerCollisionSystem : SystemBase
{
    private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;
    public EventHandler OnPlayersDestroyed;


    protected override void OnCreate()
    {
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        Entities.WithChangeFilter<OnCollision>().WithAny<PlayerTag>().ForEach((Entity thisEntity, ref OnCollision onCollision, ref PlayerLivesData playerLivesData,
            ref OnDestroyed onDestroyed, ref OnPlayerShot onPlayerShot, ref Translation trans, 
            ref MoveSpeedData moveSpeedData) =>
        {
            if (onCollision.Value)
            {
                onCollision.Value = false;
                onPlayerShot.value = true;
                
                if (playerLivesData.CanTakeDamage)
                {
                    playerLivesData.CurrentLives -= 1;
                    if (playerLivesData.CurrentLives < 1)
                    {
                        onDestroyed.Value = true;
                    }
                    else
                    {
                        trans.Value = playerLivesData.OriginPosition;
                        moveSpeedData.movementSpeed = 0;
                    }
                }
            }
        }).ScheduleParallel();
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

       
        var playerCount = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>()).CalculateEntityCount();
        if (playerCount == 0)
        {
            gameState.GameState = GameStateData.State.PlayersDead;
            SetSingleton(gameState);
            OnPlayersDestroyed(this, EventArgs.Empty);
        }
        

    }
}
