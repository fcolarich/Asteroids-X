using System;
using Unity.Entities;
using UnityEngine;

public class UIUpdateSystem : SystemBase
{
    private BeginFixedStepSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;
    public EventHandler OnPointsUpdatePlayer1;
    public EventHandler OnPointsUpdatePlayer2;
    public EventHandler OnLivesUpdatePlayer1;
    public EventHandler OnLivesUpdatePlayer2;


    protected override void OnCreate()
    {
        _beginSimulationEcbSystem = World.GetExistingSystem<BeginFixedStepSimulationEntityCommandBufferSystem>();
    }

   protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
      
        var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();

        Entities.WithChangeFilter<PlayerPointsData>().WithAll<PlayerTag>()
            .ForEach((Entity thisEntity, in PlayerPointsData playerPointsData) =>
            {
                if (HasComponent<Player1Tag>(thisEntity))
                {
                    OnPointsUpdatePlayer1(playerPointsData.points, EventArgs.Empty);                    
                }
                else
                {
                    OnPointsUpdatePlayer2(playerPointsData.points,EventArgs.Empty);                    
                }
            
            }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<PlayerLivesData>().WithAll<PlayerTag>().ForEach((Entity thisEntity, in PlayerLivesData playerLivesData) =>
        {
            if (HasComponent<Player1Tag>(thisEntity))
            {
                OnLivesUpdatePlayer1(playerLivesData.CurrentLives, EventArgs.Empty);
            }
            else
            {
                OnLivesUpdatePlayer2(playerLivesData.CurrentLives, EventArgs.Empty);
            }
        }).WithoutBurst().Run();


        _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

    }
}