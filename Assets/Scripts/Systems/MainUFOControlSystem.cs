using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

public class MainUFOControlSystem : SystemBase
{
    private NativeArray<Entity> targetEntities;
    private bool hasTarget = false;


    protected override void OnUpdate()
    {
        if (!hasTarget)
        {
            var i = 0;
            Entities.WithAny<Player1Tag>().WithAny<Player2Tag>().ForEach((Entity thisEntity) =>
            {
               // targetEntities.CopyFrom(thisEntity);
                i += 1;
            }).WithoutBurst().Run();
            hasTarget = true;
        }
        
        var localTargetEntities = targetEntities;
        Entities.ForEach((ref UFOGeneralData ufoGeneralData, ref Rotation rot, ref MoveSpeedData moveSpeedData, in MoveSpeedModifierData moveSpeedModifier, in Translation trans) => {

            if (EntityManager.Exists(ufoGeneralData.targetEntity))
            {
                var targetPosition = EntityManager.GetComponentData<Translation>(ufoGeneralData.targetEntity).Value;
                var targetDirection = targetPosition - trans.Value;
                targetDirection.y = 0f;
                ufoGeneralData.targetDirection = targetDirection;
                rot.Value = quaternion.LookRotation(targetDirection, math.up());
                moveSpeedData.movementSpeed = math.forward(rot.Value) * moveSpeedModifier.SpeedModifier;
            }
            else
            {
                ufoGeneralData.targetEntity = targetEntities[Random.Range(0, 1)];
            }
        }).WithoutBurst().Run();
    }
}
