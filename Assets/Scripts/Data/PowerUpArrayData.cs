using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct PowerUpArrayData : IComponentData
{
    public BlobAssetReference<BlobArray<Entity>> PowerUpArray;
}