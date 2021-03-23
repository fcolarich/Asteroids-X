using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public class OnHitParticlesData : IComponentData
{
    public Entity LinkedParticleEntity;
    public GameObject ParticlePrefabObject;
}