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
      
        Entities.WithChangeFilter<PlayerPointsData>().WithAll<PlayerTag>()
            .ForEach((Entity thisEntity, in PlayerPointsData playerPointsData, in PlayerTag playerTag) =>
            {
                if (playerTag.IsPlayer1)
                {
                    OnPointsUpdatePlayer1(playerPointsData.points, EventArgs.Empty);                    
                }
                else
                {
                    OnPointsUpdatePlayer2(playerPointsData.points,EventArgs.Empty);                    
                }
            }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<PlayerLivesData>().WithAll<PlayerTag>().ForEach((Entity thisEntity, in PlayerLivesData playerLivesData, in PlayerTag playerTag) =>
        {
            if (playerTag.IsPlayer1)
            {
                OnLivesUpdatePlayer1(playerLivesData.CurrentLives, EventArgs.Empty);
            }
            else
            {
                OnLivesUpdatePlayer2(playerLivesData.CurrentLives, EventArgs.Empty);
            }
        }).WithoutBurst().Run();
    }
}