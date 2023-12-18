
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
//using Unity.Transforms;


//public struct ClimbComponent : IComponentData
//{
//    public float MaxClimbHeight;
//}
namespace ImaginaryReactor
{

    public struct VaultComponent : IComponentData
    {
        public float VaultDuration;
        public CollisionFilter Filter;
        public float3 VaultOffset;
    }
    public struct Vaulting : IComponentData
    {
        public float VaultRemainTime;
        public float3 VaultStartPosition;
        public float3 VaultTargetPosition;
    }
    public struct CancelVaulting : IComponentData
    {

    }


    public struct VaultHand : IBufferElementData
    {
        public Entity ValutRayStart;
        public Entity ValutRayEnd;
    }
}