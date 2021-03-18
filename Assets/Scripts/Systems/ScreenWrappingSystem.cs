using Unity.Entities;
using UnityEngine;
using Unity.Transforms;


public partial class ScreenWrappingSystem : SystemBase
{
    private float cameraMaxHeight;
    private float cameraMinHeight;
    private float cameraMaxWidth;
    private float cameraMinWidth;
    private Vector3 camera3;

    protected override void OnStartRunning()
    {
        Entities.WithAll<Camera>().ForEach((Camera camera) =>
            {
                var cameraMax = camera.ViewportToWorldPoint(new Vector2(1,1));
                var cameraMin = camera.ViewportToWorldPoint(new Vector2(0,0));
                camera3 = cameraMax;
                cameraMaxHeight = cameraMax.y + 2;
                cameraMaxWidth = cameraMax.x + 2;
                cameraMinHeight = cameraMin.y - 2;
                cameraMinWidth = cameraMin.x - 2;
            }
        ).WithoutBurst().Run();
    }


    protected override void OnUpdate()
    {
        Entities.WithAll<MoveSpeedData>().ForEach((ref Translation trans, in LocalToWorld localToWorld)  => 
                  {
                     if (localToWorld.Position.y > cameraMaxHeight)
                     {
                         trans.Value.y = cameraMinHeight;
                     }
                     else if (localToWorld.Position.y < cameraMinHeight)
                     {
                         trans.Value.y = cameraMaxHeight;
                     }
                     
                     if (localToWorld.Position.x > cameraMaxWidth)
                     {
                         trans.Value.x = cameraMinWidth;
                     } 
                     else if (localToWorld.Position.x < cameraMinWidth)
                     {
                         trans.Value.x = cameraMaxWidth;
                     }
                  }
                  ).WithoutBurst().Run();
    }
}