using Unity.Entities;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    public struct HitscanLineComponent : IComponentData
    {
        public float3 StartPos;
        public float3 EndPos;
    }
}