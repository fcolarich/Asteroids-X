using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class InitializeAsteroidsSystem : SystemBase
{

    protected override void OnUpdate()
    {
        Entities.WithChangeFilter<AsteroidsTag>().ForEach(
            (ref Rotation rot, ref MoveSpeedData moveSpeed, 
                ref AsteroidsTag asteroidsTag, in MoveSpeedModifierData speedModifier) =>
            {
                rot.Value = Random.rotation;

                var randomSpeedX = Random.Range(-1f, 1f) * speedModifier.SpeedModifier;
                var speedY = Mathf.Sqrt(Mathf.Pow(speedModifier.SpeedModifier, 2) - Mathf.Pow(randomSpeedX, 2));
                speedY = speedY * (Random.value < 0.5f ? -1 : 1);
                var speedFloat3 = new float3(Random.Range(0.9f, 1.1f) * randomSpeedX, Random.Range(0.9f, 1.1f) * speedY,
                    0);
                moveSpeed.movementSpeed = speedFloat3;
            }).Run();
    }
}


