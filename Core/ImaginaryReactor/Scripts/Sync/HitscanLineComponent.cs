using Unity.Entities;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    public struct HitscanLineComponent : IComponentData
    {
        public int ID;
        public float3 StartPos;
        public float3 EndPos;
    }
}