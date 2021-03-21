using Unity.Entities;
using Unity.Transforms;

[UpdateAfter(typeof(PlayerInputSystem))]
public partial class GeneralMovementSystem : SystemBase
{

   protected override void OnUpdate()
   {
      if (!HasSingleton<GameStateData>()) return;
      var gameState = GetSingleton<GameStateData>();
      if (gameState.GameState != GameStateData.State.Playing) return;
      
      var deltaTime = Time.DeltaTime;
      Entities.ForEach((ref Translation trans, in MoveSpeedData moveSpeed) =>
      {
         trans.Value += moveSpeed.movementSpeed * deltaTime;
      }).ScheduleParallel();
   }
}
