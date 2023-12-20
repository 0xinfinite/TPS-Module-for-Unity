
using Unity.Entities;
using UnityEngine;

namespace ImaginaryReactor
{
    public struct Pitcher : IComponentData
    {
        public Entity GrabEntity;
        //public Entity CurrentGrabEntity;
        public Entity Hand;
        public float ThrowingPower;
    }
}
