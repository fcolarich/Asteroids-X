using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class GeneralRotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var time = Time.DeltaTime;
        Entities.WithAll<MoveRotationData>().ForEach((ref Rotation rot, in MoveRotationData moveRotation) =>
        {
            rot.Value = moveRotation.rotation;
        }).WithoutBurst().Run();
    }
}
