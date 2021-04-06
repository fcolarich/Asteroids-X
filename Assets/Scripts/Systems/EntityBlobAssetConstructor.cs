using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class EntityBlobAssetConstructor : SystemBase
{
    private Entity _entity;
   
    protected override void OnUpdate()
    {
        Entity entity = _entity;
        Entities.ForEach((Entity thisEntity, ref PowerUpArrayData powerUpArrayData,
            in PowerUpArrayClass powerUpArrayClass) =>
        {
            entity = thisEntity;
            using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
            {
                ref BlobArray<Entity> entityBlobArray = ref blobBuilder.ConstructRoot<BlobArray<Entity>>();
                BlobBuilderArray<Entity> entityArray =
                    blobBuilder.Allocate(ref entityBlobArray, powerUpArrayClass.PowerUpArray.Length);
                for (int i = 0; i < powerUpArrayClass.PowerUpArray.Length; i++)
                {
                    entityArray[i] = powerUpArrayClass.PowerUpArray[i];
                }
                powerUpArrayData.PowerUpArray =
                    blobBuilder.CreateBlobAssetReference<BlobArray<Entity>>(Allocator.Persistent);
            }
        }).WithoutBurst().Run();
        
        EntityManager.RemoveComponent<PowerUpArrayClass>(entity);
        
    }
}
