using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerPointsToLivesSystem : SystemBase
{
    protected override void OnUpdate()
    {

        Entities.WithChangeFilter<PlayerPointsData>().ForEach((ref PlayerLivesData playerLivesData, ref PlayerPointsData playerPointsData) => {

            if (playerLivesData.EarnedLives * 1000 < playerPointsData.points)
            {
                playerLivesData.CurrentLives += 1;
                playerLivesData.EarnedLives += 1;
            }

        }).Schedule();
    }
}
