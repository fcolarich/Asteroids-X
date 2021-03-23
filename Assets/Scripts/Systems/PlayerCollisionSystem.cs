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
    public EventHandler OnLivesUpdatePlayer1;
    public EventHandler OnLivesUpdatePlayer2;
    public EventHandler OnPlayersDestroyed;
    public EventHandler OnPlayerShot;
    private Pooler _pooler; 


    protected override void OnCreate()
    {
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        _pooler = Pooler.Instance;
    }

    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();

        Entities.WithAll<HasCollidedTag>().ForEach((Entity thisEntity, ref PlayerLivesData playerLivesData,
            ref CollisionControlData collisionControlData, ref Translation trans, ref MoveSpeedData moveSpeedData, in OnHitParticlesData particlesData) =>
        {
            if (playerLivesData.CanTakeDamage)
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

                OnPlayerShot(this, EventArgs.Empty);
                _pooler.Spawn(particlesData.ParticlePrefabObject, trans.Value, quaternion.identity);

                if (playerLivesData.CurrentLives < 1)
                {
                    ecb.DestroyEntity(thisEntity);
                }
                else
                {
                    trans.Value = playerLivesData.OriginPosition;
                    moveSpeedData.movementSpeed = 0;
                }
            }
            ecb.RemoveComponent<HasCollidedTag>(thisEntity);
        }).WithoutBurst().Run();
        
        Entities.WithAll<PlayerBulletTag>().WithAll<HasCollidedTag>().ForEach((Entity thisEntity,
            in CollisionControlData collisionControlData, in Translation trans, in OnHitParticlesData particlesData) =>
        {
                _pooler.Spawn(particlesData.ParticlePrefabObject,trans.Value,quaternion.identity);
                ecb.DestroyEntity(thisEntity);
        }).WithoutBurst().Run();
        
        
        var playerCount = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>()).CalculateEntityCount();
        if (playerCount == 0)
        {
            gameState.GameState = GameStateData.State.PlayersDead;
            SetSingleton(gameState);
            OnPlayersDestroyed(this, EventArgs.Empty);
        }
        
        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}
