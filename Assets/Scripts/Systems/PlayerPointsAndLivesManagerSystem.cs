using System;
using Unity.Entities;


public class PlayerPointsAndLivesManagerSystem : SystemBase
{


    protected override void OnUpdate()
    {

        Entities.WithChangeFilter<PlayerPointsData>().ForEach((Entity thisEntity,ref PlayerLivesData playerLivesData, ref PlayerPointsData playerPointsData) => {
            if (playerLivesData.EarnedLives * 1000 < playerPointsData.points)
            {
                playerLivesData.CurrentLives += 1;
                playerLivesData.EarnedLives += 1;
            }
        }).ScheduleParallel();
    }
}
