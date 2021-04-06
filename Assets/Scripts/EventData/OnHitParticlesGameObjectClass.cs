using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public class OnHitParticlesGameObjectClass : IComponentData
{
    public GameObject ParticlePrefabObject;
}