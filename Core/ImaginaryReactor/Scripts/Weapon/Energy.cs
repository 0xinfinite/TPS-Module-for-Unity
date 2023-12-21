using Unity.Entities;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    public struct Energy : IComponentData
    {
        public float3 SourcePosition;
        public float3 ForcePosition;
        public float3 ForceNormal;
        public float3 ForceNormalPhysically;
        public float BaseDamage;
        public float CriticalDamage;
    }

    public struct StackedEnergy : IBufferElementData {
        public Energy Energy;
    }
}