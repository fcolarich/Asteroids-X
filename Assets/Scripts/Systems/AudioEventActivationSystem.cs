using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class AudioEventActivationSystem : SystemBase
{
    public EventHandler OnBulletFire;
    public EventHandler OnEnemyHit;
    public EventHandler OnBigShipDestroyed;
    public EventHandler OnPlayerShot;
    public EventHandler OnEnemyShipCreated;
    public EventHandler OnEnemyBigShipCreated;

    protected override void OnUpdate()
    {
        
        Entities.WithChangeFilter<OnEnemyShipCreated>().ForEach((ref OnEnemyShipCreated onEnemyShipCreated) =>
        {
            if (onEnemyShipCreated.Value)
            {
                OnEnemyShipCreated(this, EventArgs.Empty);
                onEnemyShipCreated.Value= false;
            }
        }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<OnEnemyBigShipCreated>().ForEach((ref OnEnemyBigShipCreated onEnemyBigShipCreated) =>
        {
            if (onEnemyBigShipCreated.Value)
            {
                OnEnemyBigShipCreated(this, EventArgs.Empty);
                onEnemyBigShipCreated.Value= false;
            }
        }).WithoutBurst().Run();

        Entities.WithChangeFilter<OnBulletFired>().ForEach((ref OnBulletFired onBulletFired) =>
        {
            if (onBulletFired.Value)
            {
                OnBulletFire(onBulletFired, EventArgs.Empty);
                onBulletFired.Value= false;
            }
        }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<OnPlayerShot>().ForEach((ref OnPlayerShot onPlayerShot, in OnHitParticlesData particlesData, in Translation trans) =>
        {
            if (onPlayerShot.value)
            {
                OnPlayerShot(this, EventArgs.Empty);
                onPlayerShot.value = false;
                Pooler.Instance.Spawn(particlesData.ParticlePrefabObject, trans.Value, quaternion.identity);
            }
        }).WithoutBurst().Run();
        
        
        Entities.WithChangeFilter<OnEnemyHit>().WithNone<UFOBigTag>().ForEach((ref OnDestroyed onDestroyed, ref OnEnemyHit onEnemyHit, in OnHitParticlesData particlesData, in Translation trans) =>
        {
            if (onEnemyHit.Value)
            {
                OnEnemyHit(this, EventArgs.Empty);
                Pooler.Instance.Spawn(particlesData.ParticlePrefabObject, trans.Value, quaternion.identity);
                onEnemyHit.Value= false;
                onDestroyed.Value = true;
            }
        }).WithoutBurst().Run();
        
        Entities.WithChangeFilter<OnEnemyHit>().WithAll<UFOBigTag>().ForEach((ref OnEnemyHit onEnemyHit, ref UFOLivesData ufoLivesData, in OnHitParticlesData particlesData, in Translation trans) =>
        {
            if (onEnemyHit.Value)
            {
                Pooler.Instance.Spawn(particlesData.ParticlePrefabObject, trans.Value, quaternion.identity);
                onEnemyHit.Value= false;
                if (ufoLivesData.CurrentLives - 1 < 0)
                {
                    OnBigShipDestroyed(this, EventArgs.Empty);
                }
                else
                {
                    OnEnemyHit(this, EventArgs.Empty);
                }

            }
        }).WithoutBurst().Run();
                            
        
        
        
    }
}

