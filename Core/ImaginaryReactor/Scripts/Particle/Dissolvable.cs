
using System;
using Unity.Entities;

namespace ImaginaryReactor
{
    [Serializable]
    public struct Dissolvable : IComponentData
    {
        public float RemainTime;
    }
}