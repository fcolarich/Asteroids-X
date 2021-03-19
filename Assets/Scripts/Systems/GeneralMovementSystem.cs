using Unity.Entities;
using Unity.Transforms;


public partial class GeneralMovementSystem : SystemBase
{

   protected override void OnUpdate()
   {
      var deltaTime = Time.DeltaTime;
      Entities.ForEach((ref Translation trans, in MoveSpeedData moveSpeed) =>
         {
            trans.Value += moveSpeed.movementSpeed * deltaTime;
         }).ScheduleParallel();
   }
}
