using System;
using Unity.Entities;


public class PlayerPointsAndLivesManagerSystem : SystemBase
{
    public EventHandler OnPointsUpdatePlayer1;
    public EventHandler OnPointsUpdatePlayer2;


    protected override void OnUpdate()
    {

        Entities.WithChangeFilter<PlayerPointsData>().ForEach((Entity thisEntity,ref PlayerLivesData playerLivesData, ref PlayerPointsData playerPointsData) => {

            if (HasComponent<Player1Tag>(thisEntity))
            {
                OnPointsUpdatePlayer1.Invoke(playerPointsData.points, EventArgs.Empty);
            }
            else
            {
                OnPointsUpdatePlayer2.Invoke(playerPointsData.points, EventArgs.Empty);

            }
            

            if (playerLivesData.EarnedLives * 1000 < playerPointsData.points)
            {
                playerLivesData.CurrentLives += 1;
                playerLivesData.EarnedLives += 1;
            }

        }).WithoutBurst().Run();
    }
}
