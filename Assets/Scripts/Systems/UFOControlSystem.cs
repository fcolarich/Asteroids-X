using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class UFOControlSystem : SystemBase
{
    private NativeArray<Entity> _targetEntities;


    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        _targetEntities = GetEntityQuery(ComponentType.ReadOnly<PlayerTag>()).ToEntityArray(Allocator.Temp);
        
        var localTargetEntities = _targetEntities;
        var _localTranslation = GetComponentDataFromEntity<Translation>();
        
        Entities.ForEach((Entity thisEntity, ref UFOGeneralData ufoGeneralData, ref Rotation rot,
            ref MoveSpeedData moveSpeedData, in MoveSpeedModifierData moveSpeedModifier, in Translation trans) =>
        {
            if (HasComponent<PlayerTag>(ufoGeneralData.targetEntity))
            {
                var targetPosition = _localTranslation[ufoGeneralData.targetEntity].Value;
                var targetDirection = targetPosition-trans.Value;
                ufoGeneralData.targetDirection = targetDirection;
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
                    int randomInt = Random.Range(0, localTargetEntities.Length);
                    ufoGeneralData.targetEntity = localTargetEntities[randomInt];
                }
            }
        }).Run();
    }
}
