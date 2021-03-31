using Unity.Entities;
using UnityEngine;


[GenerateAuthoringComponent]

public struct RandomNumberData : IComponentData
{
    public float Random;
}