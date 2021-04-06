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
        
        var screenWrappingSystem = World.GetExistingSystem<ScreenWrappingSystem>();
        var cameraMaxHeight = screenWrappingSystem._cameraMaxHeight;
        var cameraMinHeight = screenWrappingSystem._cameraMinHeight;
        var cameraMaxWidth  = screenWrappingSystem._cameraMaxWidth;
        var cameraMinWidth = screenWrappingSystem._cameraMinWidth;
        var cameraWidth = cameraMaxWidth - cameraMinWidth;
        var cameraHeight = cameraMaxHeight - cameraMinHeight;
        
        
        var localTargetEntities = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>()).ToEntityArray(Allocator.TempJob);
        var localTranslation = GetComponentDataFromEntity<Translation>(true);
        var deltaTime = Time.DeltaTime;
        
        Entities.WithReadOnly(localTranslation).WithReadOnly(localTargetEntities).WithDisposeOnCompletion(localTargetEntities).ForEach((Entity thisEntity, 
            ref UFOGeneralData ufoGeneralData, ref Rotation rot, ref MoveSpeedData moveSpeedData, in UFOTag ufoTag,
            in MoveSpeedModifierData moveSpeedModifier, in Translation trans) =>
        {
            if (HasComponent<PlayerTag>(ufoGeneralData.TargetEntity))
            {
                var targetPosition = localTranslation[ufoGeneralData.TargetEntity].Value;
                
                var x = targetPosition.x - trans.Value.x;
                var y = targetPosition.y - trans.Value.y;
                float directionX;
                float directionY;
                if (Mathf.Abs(x) > cameraWidth / 2)
                {
                    if (x > 0)
                    {
                        directionX = - cameraWidth + x;
                    }
                    else
                    {
                        directionX = cameraWidth - x;    
                    }
                }
                else
                {
                    directionX = x;
                }
                if (Mathf.Abs(y) > cameraHeight / 2)
                {
                    if (y > 0)
                    {
                        directionY = - cameraHeight + y;
                    }
                    else
                    {
                        directionY = cameraHeight - y;
                    }
                }
                else
                {
                    directionY = y;
                }

                var targetDirection = new float3(directionX, directionY, 0);


                 var rotation= quaternion.LookRotationSafe(targetDirection, math.down());
                
                if (ufoTag.IsSmallUFO)
                {
                    rot.Value = Quaternion.Lerp(rot.Value, rotation, deltaTime);
                    moveSpeedData.movementSpeed = math.forward(rot.Value) * moveSpeedModifier.SpeedModifier;
                }
                else if (ufoTag.IsMediumUFO)
                {
                    var angle = Quaternion.Angle(rot.Value, rotation);
                    rot.Value = Quaternion.Lerp(rot.Value, rotation, deltaTime*angle/5f);
                    moveSpeedData.movementSpeed = math.left() * moveSpeedModifier.SpeedModifier;
                }
                else if (ufoTag.IsBigUFO)
                {
                    rot.Value = Quaternion.Lerp(rot.Value, rotation, deltaTime);
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
