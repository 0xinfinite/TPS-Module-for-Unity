
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace ImaginaryReactor
{
    public class MagicIFF_Authoring : MonoBehaviour
    {
        public CustomPhysicsMaterialTags Key;

        public class Baker : Baker<MagicIFF_Authoring>
        {
            public override void Bake(MagicIFF_Authoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

                AddComponent(entity, new MagicIFF() { Key = authoring.Key.Value });
            }
        }

    }

    public struct MagicIFF : IComponentData
    {
        public ColliderKey Key;
    }
}