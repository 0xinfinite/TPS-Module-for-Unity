using Unity.Entities;
using Unity.Mathematics;

public struct HitscanLineComponent : IComponentData
{
    public float3 StartPos;
    public float3 EndPos;
}
