
using System;
using Unity.Entities;

[Serializable]
public struct Dissolvable : IComponentData
{
    public float RemainTime;
}
