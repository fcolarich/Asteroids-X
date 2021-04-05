using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class InitializeAsteroidsSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem _endSimulationECBSystem;

    protected override void OnCreate()
    {
        _endSimulationECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var ecb = _endSimulationECBSystem.CreateCommandBuffer().AsParallelWriter();

        var elapsedTime = Time.ElapsedTime;
        Entities.WithChangeFilter<ToInitialize>().WithAll<AsteroidsTag>().ForEach(
            (int entityInQueryIndex,ref ToInitialize toInitializeTag, ref Rotation rot, 
                ref MoveSpeedData moveSpeed, 
                in MoveSpeedModifierData speedModifier) =>
            {
                if (toInitializeTag.Value)
                {
                    var index = Random.CreateFromIndex(Convert.ToUInt32((1+entityInQueryIndex)*elapsedTime)).NextUInt();
                    moveSpeed.movementSpeed = GenerateRandomSpeed(speedModifier,index);
                    rot.Value = Random.CreateFromIndex(index).NextQuaternionRotation();
                    toInitializeTag.Value = false;
                }
            }).ScheduleParallel();
        
        _endSimulationECBSystem.AddJobHandleForProducer(this.Dependency);

    }
    
    static float3 GenerateRandomSpeed(MoveSpeedModifierData speedModifier, uint index)
    {
        var randomSpeedX = Random.CreateFromIndex(index).NextFloat(-1f, 1f) * speedModifier.SpeedModifier;
        var speedY = Mathf.Sqrt(Mathf.Pow(speedModifier.SpeedModifier, 2) - Mathf.Pow(randomSpeedX, 2));
        speedY = speedY * (Random.CreateFromIndex(index).NextFloat() < 0.5f ? -1 : 1);
        var speedFloat3 = new float3(Random.CreateFromIndex(index).NextFloat(0.9f, 1.5f) * randomSpeedX, Random.CreateFromIndex(index).NextFloat(0.9f, 1.5f) * speedY,
            0);
        return speedFloat3;
    }
    
}




