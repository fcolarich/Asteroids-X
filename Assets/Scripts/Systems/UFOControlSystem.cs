using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UFOControlSystem : SystemBase
{


    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var localTargetEntities = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>()).ToEntityArray(Allocator.TempJob);
        var localTranslation = GetComponentDataFromEntity<Translation>(true);
        
        Entities.WithReadOnly(localTranslation).WithReadOnly(localTargetEntities).WithDisposeOnCompletion(localTargetEntities).ForEach((Entity thisEntity, 
            ref UFOGeneralData ufoGeneralData, ref Rotation rot, ref MoveSpeedData moveSpeedData, 
            in MoveSpeedModifierData moveSpeedModifier, in Translation trans) =>
        {
            if (HasComponent<PlayerTag>(ufoGeneralData.TargetEntity))
            {
                var targetPosition = localTranslation[ufoGeneralData.TargetEntity].Value;
                var targetDirection = targetPosition-trans.Value;
                ufoGeneralData.TargetDirection = targetDirection;
                rot.Value = quaternion.LookRotationSafe(targetDirection, math.down());
                if (HasComponent<UFOSmallTag>(thisEntity))
                {
                    moveSpeedData.movementSpeed = math.forward(rot.Value) * moveSpeedModifier.SpeedModifier;
                }
                else if (HasComponent<UFOMediumTag>(thisEntity))
                {
                    moveSpeedData.movementSpeed = math.left() * moveSpeedModifier.SpeedModifier;
                }
                else if (HasComponent<UFOBigTag>(thisEntity))
                {
                    if (Vector3.Distance(trans.Value, targetPosition) > 60)
                    {
                        moveSpeedData.movementSpeed = math.forward(rot.Value) * moveSpeedModifier.SpeedModifier;
                    }
                    else
                    {
                        moveSpeedData.movementSpeed = math.forward(rot.Value);
                    }
                }
            }
            else
            {
                if (localTargetEntities.Length > 0)
                {
                    int randomInt = Unity.Mathematics.Random.CreateFromIndex(1).NextInt(0, localTargetEntities.Length);
                    ufoGeneralData.TargetEntity = localTargetEntities[randomInt];
                }
            }
        }).ScheduleParallel();
    }
}
