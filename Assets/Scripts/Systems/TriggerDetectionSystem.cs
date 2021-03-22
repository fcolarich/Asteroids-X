using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

public class TriggerDetectionSystem : JobComponentSystem
{
    private BuildPhysicsWorld _buildPhysicsWorld;
    private StepPhysicsWorld _stepPhysicsWorld;

    protected override void OnCreate()
    {
        _buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }


    struct OnTriggerSystemJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<AsteroidsTag> asteroidTag;
        [ReadOnly] public ComponentDataFromEntity<Player1Tag> player1Tag;
        [ReadOnly] public ComponentDataFromEntity<Player2Tag> player2Tag;
        [ReadOnly] public ComponentDataFromEntity<PowerUpTag> powerUpTag;
        [ReadOnly] public ComponentDataFromEntity<BulletSourceData> bulletSourceData;
        public EntityManager entityManager;

        public ComponentDataFromEntity<CollisionControlData> collisionControl;
        private CollisionControlData CollisionControlData;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            bool powerUP = false;

            CollisionControlData.HasCollided = true;

            if (player1Tag.HasComponent(entityA) || player2Tag.HasComponent(entityA) ||
                player1Tag.HasComponent(entityB) || player2Tag.HasComponent(entityB))
            {
                if (powerUpTag.HasComponent(entityA))
                {
                    CollisionControlData.AffectedTarget = entityB;
                    collisionControl[entityA] = CollisionControlData;
                    powerUP = true;
                }
                else if (powerUpTag.HasComponent(entityB))
                {
                    CollisionControlData.AffectedTarget = entityA;
                    collisionControl[entityB] = CollisionControlData;
                    powerUP = true;
                }
            }
            else if (asteroidTag.HasComponent(entityA))
            {
                if (entityManager.Exists(entityB))
                {
                    if (entityManager.Exists(bulletSourceData[entityB].Source))
                    {
                        CollisionControlData.AffectedTarget = bulletSourceData[entityB].Source;
                    }
                }
            }
            else if (asteroidTag.HasComponent(entityB))
            {
                if (entityManager.Exists(entityA))
                {
                    if (entityManager.Exists(bulletSourceData[entityA].Source))
                    {
                        CollisionControlData.AffectedTarget = bulletSourceData[entityA].Source;
                    }
                }
            }
            if (!powerUP)
            {
                if (entityManager.Exists(entityA))
                {
                    collisionControl[entityA] = CollisionControlData;
                }
                if (entityManager.Exists(entityB))
                {
                    collisionControl[entityB] = CollisionControlData;
                }
            }
        }
    }



    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {

        if (!HasSingleton<GameStateData>())
        {
            return new JobHandle();
        }
        var gameState = GetSingleton<GameStateData>();
        if (gameState.GameState != GameStateData.State.Playing)
        {
            return new JobHandle();
        };

        var job = new OnTriggerSystemJob
        {
            asteroidTag = GetComponentDataFromEntity<AsteroidsTag>(true),
            player1Tag = GetComponentDataFromEntity<Player1Tag>(true),
            player2Tag = GetComponentDataFromEntity<Player2Tag>(true),
            powerUpTag = GetComponentDataFromEntity<PowerUpTag>(true),
            collisionControl = GetComponentDataFromEntity<CollisionControlData>(),
            bulletSourceData = GetComponentDataFromEntity<BulletSourceData>(true),
            entityManager = EntityManager
        };



        JobHandle jobHandle = job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);

        jobHandle.Complete();
        return jobHandle;
    }
}
