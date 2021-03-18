using Unity.Entities;
using Unity.Transforms;


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
