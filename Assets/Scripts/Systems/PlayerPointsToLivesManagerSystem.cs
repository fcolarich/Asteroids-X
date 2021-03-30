using System;
using Unity.Entities;


public class PlayerPointsToLivesManagerSystem : SystemBase
{
    protected override void OnUpdate()
    {

        Entities.WithChangeFilter<PlayerPointsData>().ForEach((ref PlayerLivesData playerLivesData, in PlayerPointsData playerPointsData) => {
            if (playerLivesData.EarnedLives * 1000 < playerPointsData.points)
            {
                playerLivesData.CurrentLives += 1;
                playerLivesData.EarnedLives += 1;
            }
        }).ScheduleParallel();
    }
}
