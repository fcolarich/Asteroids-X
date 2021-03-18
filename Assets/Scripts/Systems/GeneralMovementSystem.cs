using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;


public partial class GeneralMovementSystem : SystemBase
{

   protected override void OnUpdate()
   {
      var deltaTime = Time.DeltaTime;
      Entities.WithAll<MoveSpeedData>().ForEach((ref Translation trans, ref MoveSpeedData moveSpeed) =>
         {
            trans.Value += moveSpeed.movementSpeed * deltaTime;
         }).ScheduleParallel();
   }
}
