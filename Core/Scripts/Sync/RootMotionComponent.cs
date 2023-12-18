using Unity.Entities;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    public struct RootMotionComponent : IComponentData
    {
        public float3 Velocity;

    }
}
