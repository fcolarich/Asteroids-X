using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(PlayerInputSystem))]
public partial class ParticleFollowTargetSystem : SystemBase
{
   private BeginSimulationEntityCommandBufferSystem _beginSimulationEcbSystem;

   protected override void OnCreate()
   {
      _beginSimulationEcbSystem = World.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
   }

   
   protected override void OnUpdate()
   {
      if (!HasSingleton<GameStateData>()) return;
      var gameState = GetSingleton<GameStateData>();
      if (gameState.GameState != GameStateData.State.Playing) return;

      
      var deltaTime = Time.DeltaTime;
      var ecb = _beginSimulationEcbSystem.CreateCommandBuffer();


      var translation = GetComponentDataFromEntity<Translation>(true);
      
      Entities.WithAll<ParticleLinkTag>()
         .ForEach((Entity thisEntity, in LinkedParticleData linkedParticleData) =>
      {
         var targetTranslation = translation[linkedParticleData.Target].Value;
         linkedParticleData.ParticleObject.transform.position = targetTranslation;
         
       linkedParticleData.TimerToDestroy -= deltaTime;
         if (linkedParticleData.TimerToDestroy < 0)
         {
            Pooler.Instance.DeSpawn(linkedParticleData.ParticleObject);
            ecb.DestroyEntity(thisEntity);
         }
      }).WithoutBurst().Run();
      
      _beginSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

   }
}
