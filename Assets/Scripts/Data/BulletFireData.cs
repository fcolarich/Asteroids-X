using Unity.Entities;
using UnityEngine;


[GenerateAuthoringComponent]

public struct BulletFireData : IComponentData
{
    public int MaxBullets;
    public int CurrentBullets;
    public float SecondsBetweenBullets;
    public float BulletTimer;
    public bool TryFire;
    public Entity BulletPrefab;
    public float BulletGroupTimer;
    public float SecondsBetweenBulletGroups;
    public float BulletSpeed;
}