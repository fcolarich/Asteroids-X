using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[UpdateAfter(typeof(PlayerInputSystem))]
public class TestingSystem : SystemBase
{

    protected override void OnUpdate()
    {
        Entities.WithAll<CompanionData>().ForEach((Entity thisEntity, ref Translation trans) =>
        {
            var objectO = EntityManager.GetComponentObject<SpriteRenderer>(thisEntity);
            //var emission = objectO.emission;
            //emission.enabled = true;
            trans.Value += Time.DeltaTime * math.forward();
            Debug.Log(objectO.name);
        }).WithoutBurst().Run();
    }
}