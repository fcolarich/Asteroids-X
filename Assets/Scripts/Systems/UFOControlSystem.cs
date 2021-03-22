using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class UFOControlSystem : SystemBase
{
    private NativeArray<Entity> targetEntities;


    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        targetEntities = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>()).ToEntityArray(Allocator.Temp);


        var localTargetEntities = targetEntities;
        Entities.ForEach((Entity thisEntity, ref UFOGeneralData ufoGeneralData, ref Rotation rot,
            ref MoveSpeedData moveSpeedData, in MoveSpeedModifierData moveSpeedModifier, in Translation trans,
            in LocalToWorld localToWorld) =>
        {

            if (EntityManager.Exists(ufoGeneralData.targetEntity))
            {
                var targetPosition = EntityManager.GetComponentData<LocalToWorld>(ufoGeneralData.targetEntity)
                    .Position;

                var targetDirection = targetPosition - localToWorld.Position;
                ufoGeneralData.targetDirection = targetDirection;
                rot.Value = quaternion.LookRotation(targetDirection, math.up());


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
                    if (Vector3.Distance(localToWorld.Position, targetPosition) > 100)
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
                    int randomInt = Random.Range(0, localTargetEntities.Length);
                    ufoGeneralData.targetEntity = localTargetEntities[randomInt];
                }
            }
        }).WithoutBurst().Run();
    }
}