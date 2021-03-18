using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct MoveRotationData : IComponentData
{
        public quaternion rotation;
}
