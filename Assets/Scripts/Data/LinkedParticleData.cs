using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public class LinkedParticleData : IComponentData
{
    public GameObject ParticleObject;
    public Entity Target;
    public float TimerToDestroy;
}