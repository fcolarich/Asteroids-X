using Unity.Entities;
using UnityEngine;
using Unity.Transforms;


public partial class ScreenWrappingSystem : SystemBase
{
    private float _cameraMaxHeight;
    private float _cameraMinHeight;
    private float _cameraMaxWidth;
    private float _cameraMinWidth;
    private const int CAMERA_BOUNDARY = 5;
    

    protected override void OnStartRunning()
    {
        Entities.ForEach((Camera camera) =>
            {
                var cameraMax = camera.ViewportToWorldPoint(new Vector2(1,1));
                var cameraMin = camera.ViewportToWorldPoint(new Vector2(0,0));
                _cameraMaxHeight = cameraMax.y + CAMERA_BOUNDARY;
                _cameraMaxWidth = cameraMax.x + CAMERA_BOUNDARY;
                _cameraMinHeight = cameraMin.y - CAMERA_BOUNDARY;
                _cameraMinWidth = cameraMin.x - CAMERA_BOUNDARY;
            }
        ).WithoutBurst().Run();
    }


    protected override void OnUpdate()
    {
        var cameraMaxHeight = _cameraMaxHeight;
        var cameraMinHeight = _cameraMinHeight;
        var cameraMaxWidth  = _cameraMaxWidth;
        var cameraMinWidth = _cameraMinWidth;
        
        Entities.WithAll<MoveSpeedData>().WithNone<UFOBigTag>().WithNone<UFOSmallTag>().ForEach((ref Translation trans, in LocalToWorld localToWorld)  => 
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
                  ).ScheduleParallel();
    }
}