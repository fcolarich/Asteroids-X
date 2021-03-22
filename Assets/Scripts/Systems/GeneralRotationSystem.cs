using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class GeneralRotationSystem : SystemBase
{

    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState == GameStateData.State.Playing)
        {
            Entities.ForEach((ref Rotation rot, in MoveRotationData moveRotation) =>
            {
                rot.Value = moveRotation.rotation;
            }).WithoutBurst().Run();
        }

    }
}
