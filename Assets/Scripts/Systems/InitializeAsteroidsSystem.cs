using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class InitializeAsteroidsSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem _endSimulationECBSystem;

    protected override void OnCreate()
    {
        _endSimulationECBSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var ecb = _endSimulationECBSystem.CreateCommandBuffer();

        Entities.WithAll<ToInitializeTag>().WithAll<AsteroidsTag>().ForEach(
            (ref Rotation rot, ref MoveSpeedData moveSpeed, in MoveSpeedModifierData speedModifier, in Entity thisEntity) =>
            {
                moveSpeed.movementSpeed = GenerateRandomSpeed(speedModifier);
                ecb.RemoveComponent<ToInitializeTag>(thisEntity);
                rot.Value = Random.rotation;
            }).WithoutBurst().Run();
        
        _endSimulationECBSystem.AddJobHandleForProducer(this.Dependency);

    }
    
    static float3 GenerateRandomSpeed(MoveSpeedModifierData speedModifier)
    {
        var randomSpeedX = Random.Range(-1f, 1f) * speedModifier.SpeedModifier;
        var speedY = Mathf.Sqrt(Mathf.Pow(speedModifier.SpeedModifier, 2) - Mathf.Pow(randomSpeedX, 2));
        speedY = speedY * (Random.value < 0.5f ? -1 : 1);
        var speedFloat3 = new float3(Random.Range(0.9f, 1.5f) * randomSpeedX, Random.Range(0.9f, 1.5f) * speedY,
            0);
        return speedFloat3;
    }
    
}




