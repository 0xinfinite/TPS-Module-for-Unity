
using Unity.Entities;
using UnityEngine;

namespace ImaginaryReactor
{
    public struct Throwable : IComponentData
    {
        public Entity Owner;
    }
}
