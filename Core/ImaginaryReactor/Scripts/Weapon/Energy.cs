using Unity.Entities;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    public struct Energy : IComponentData
    {
        public float3 SourcePosition;
        public float3 ForcePosition;
        public float3 ForceNormal;
        public bool IsForcePoint;
        public float3 ForceVector;
        public float BaseDamage;
        public float CriticalDamage;
    }

    public struct StackedEnergy : IBufferElementData {
        public Energy Energy;
    }
}