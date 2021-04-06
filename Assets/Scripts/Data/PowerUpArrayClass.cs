using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public class PowerUpArrayClass : IComponentData
{
    public Entity[] PowerUpArray;
}

