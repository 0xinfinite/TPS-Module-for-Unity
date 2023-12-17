using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct LaserPointer : IComponentData
{
    public Entity Owner;
}
