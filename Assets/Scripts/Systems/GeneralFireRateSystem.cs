using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class GeneralFireRateSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem _endSimulationEcbSystem;
    
    public EventHandler OnBulletFire;

    
    protected override void OnCreate()
    {
        _endSimulationEcbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        if (!HasSingleton<GameStateData>()) return;
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing) return;
        
        var ecb = _endSimulationEcbSystem.CreateCommandBuffer();
        var deltaTime = Time.DeltaTime;


        Entities.ForEach((int entityInQueryIndex, Entity thisEntity, ref BulletFireData bulletFireData,
            in Translation trans, in Rotation rot, in MoveSpeedData moveSpeed) =>
        {
            if (bulletFireData.BulletTimer <= 0)
            {

                if (bulletFireData.TryFire && bulletFireData.CurrentBullets < bulletFireData.MaxBullets)
                {

                    bulletFireData.CurrentBullets += 1;
                    if (bulletFireData.CurrentBullets == 1)
                    {
                        bulletFireData.BulletGroupTimer = bulletFireData.SecondsBetweenBulletGroups;
                    }

                    bulletFireData.BulletTimer = bulletFireData.SecondsBetweenBullets;
                    var bulletEntity = ecb.Instantiate(bulletFireData.BulletPrefab);
                    OnBulletFire(this, EventArgs.Empty);
                    ecb.SetComponent(bulletEntity,
                        new Translation() {Value = trans.Value + (10 * math.forward(rot.Value))});
                    ecb.SetComponent(bulletEntity, new Rotation() {Value = rot.Value});
                    ecb.SetComponent(bulletEntity,
                        new MoveSpeedData()
                        {
                            movementSpeed = math.forward(rot.Value) * bulletFireData.BulletSpeed +
                                            moveSpeed.movementSpeed
                        });
                    ecb.SetComponent(bulletEntity, new BulletSourceData() {Source = thisEntity});
                }
            }
            else
            {
                bulletFireData.BulletTimer -= deltaTime;
            }

            if (bulletFireData.BulletGroupTimer <= 0)
            {
                bulletFireData.CurrentBullets = 0;
            }
            else
            {
                bulletFireData.BulletGroupTimer -= deltaTime;
            }

            if (HasComponent<PlayerTag>(thisEntity))
            {
                bulletFireData.TryFire = false;
            }
        }).WithoutBurst().Run();

        _endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
    }
}
