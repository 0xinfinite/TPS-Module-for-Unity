
using Unity.Entities;
using Unity.Mathematics;

namespace ImaginaryReactor
{
    public struct LoudSound : IComponentData
    {
        public float3 Source;
        public float Range;
        public float ElapsedTime;
    }

    public struct HeardSound : IComponentData
    {
        public float3 SoundSource;
        public bool playerSide;
        public bool Recognized;
    }
}